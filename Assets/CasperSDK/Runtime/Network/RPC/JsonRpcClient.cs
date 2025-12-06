using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using CasperSDK.Core.Configuration;
using Newtonsoft.Json;

namespace CasperSDK.Network.RPC
{
    /// <summary>
    /// JSON-RPC 2.0 request model
    /// </summary>
    [Serializable]
    internal class JsonRpcRequest
    {
        public string jsonrpc = "2.0";
        public int id;
        public string method;
        public object @params;
    }

    /// <summary>
    /// JSON-RPC 2.0 response model
    /// </summary>
    [Serializable]
    internal class JsonRpcResponse<T>
    {
        public string jsonrpc;
        public int id;
        public T result;
        public JsonRpcError error;
    }

    /// <summary>
    /// JSON-RPC error model
    /// </summary>
    [Serializable]
    internal class JsonRpcError
    {
        public int code;
        public string message;
        public object data;
    }

    /// <summary>
    /// JSON-RPC client for Casper Network using UnityWebRequest.
    /// Implements retry logic with exponential backoff.
    /// </summary>
    public class JsonRpcClient
    {
        private readonly string _endpoint;
        private readonly int _timeoutSeconds;
        private readonly int _maxRetries;
        private readonly bool _enableLogging;
        private int _requestIdCounter = 1;

        /// <summary>
        /// Initializes a new JSON-RPC client
        /// </summary>
        /// <param name="endpoint">RPC endpoint URL</param>
        /// <param name="config">Network configuration</param>
        public JsonRpcClient(string endpoint, NetworkConfig config)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            _endpoint = endpoint;
            _timeoutSeconds = config?.RequestTimeoutSeconds ?? 30;
            _maxRetries = config?.MaxRetryAttempts ?? 3;
            _enableLogging = config?.EnableLogging ?? false;
        }

        /// <summary>
        /// Sends a JSON-RPC request and returns the result
        /// </summary>
        /// <typeparam name="TResult">Expected result type</typeparam>
        /// <param name="method">RPC method name</param>
        /// <param name="parameters">Request parameters</param>
        /// <returns>RPC response result</returns>
        public async Task<TResult> SendRequestAsync<TResult>(string method, object parameters = null)
        {
            if (string.IsNullOrWhiteSpace(method))
            {
                throw new ArgumentNullException(nameof(method));
            }

            var request = new JsonRpcRequest
            {
                id = _requestIdCounter++,
                method = method,
                @params = parameters ?? new { } // Use empty object if parameters is null to ensure params field is included
            };

            // Try with retry logic
            Exception lastException = null;
            for (int attempt = 0; attempt <= _maxRetries; attempt++)
            {
                try
                {
                    return await SendRequestInternalAsync<TResult>(request, attempt);
                }
                catch (NetworkException ex)
                {
                    lastException = ex;
                    
                    if (attempt < _maxRetries)
                    {
                        // Exponential backoff: 1s, 2s, 4s, etc.
                        int delayMs = (int)Math.Pow(2, attempt) * 1000;
                        
                        if (_enableLogging)
                        {
                            Debug.LogWarning($"[CasperSDK] Request failed (attempt {attempt + 1}/{_maxRetries + 1}), " +
                                           $"retrying in {delayMs}ms: {ex.Message}");
                        }

                        await Task.Delay(delayMs);
                    }
                }
                catch (RpcException)
                {
                    // Don't retry RPC errors (they're server-side issues)
                    throw;
                }
            }

            // All retries exhausted
            throw new NetworkException($"Request failed after {_maxRetries + 1} attempts", lastException);
        }

        private async Task<TResult> SendRequestInternalAsync<TResult>(JsonRpcRequest request, int attemptNumber)
        {
            // Use Newtonsoft.Json for proper serialization (works with all object types)
            string jsonRequest = JsonConvert.SerializeObject(request);
            
            if (_enableLogging && attemptNumber == 0)
            {
                Debug.Log($"[CasperSDK] RPC Request: {request.method}");
                Debug.Log($"[CasperSDK] Endpoint: {_endpoint}");
                Debug.Log($"[CasperSDK] Timeout: {_timeoutSeconds}s");
                Debug.Log($"[CasperSDK] JSON Request: {jsonRequest}");
            }

            using (UnityWebRequest webRequest = new UnityWebRequest(_endpoint, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.timeout = _timeoutSeconds;
                
                Debug.Log($"[CasperSDK] Sending request to {_endpoint}...");

                // Send the request
                var operation = webRequest.SendWebRequest();
                
                var startTime = Time.realtimeSinceStartup;

                // Wait for completion
                while (!operation.isDone)
                {
                    // Stop processing if Unity exits play mode
                    if (!Application.isPlaying)
                    {
                        webRequest.Abort();
                        throw new NetworkException("Request cancelled - Unity exited play mode");
                    }
                    
                    await Task.Yield();
                    
                    // Log progress every 5 seconds
                    if (_enableLogging && (Time.realtimeSinceStartup - startTime) % 5 < 0.1f)
                    {
                        Debug.Log($"[CasperSDK] Request in progress... {Time.realtimeSinceStartup - startTime:F1}s elapsed");
                    }
                }
                
                Debug.Log($"[CasperSDK] Request completed in {Time.realtimeSinceStartup - startTime:F2}s");
                Debug.Log($"[CasperSDK] Result: {webRequest.result}");

                // Check for network errors
                if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                    webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    throw new NetworkException(
                        $"Network error: {webRequest.error} (Code: {webRequest.responseCode})");
                }

                // Parse response using Newtonsoft.Json
                string jsonResponse = webRequest.downloadHandler.text;

                if (_enableLogging)
                {
                    Debug.Log($"[CasperSDK] RPC Response: {jsonResponse.Substring(0, Math.Min(200, jsonResponse.Length))}...");
                }

                try
                {
                    var response = JsonConvert.DeserializeObject<JsonRpcResponse<TResult>>(jsonResponse);

                    // Check for RPC errors
                    if (response.error != null)
                    {
                        throw new RpcException(
                            $"RPC error: {response.error.message}",
                            response.error.code);
                    }

                    return response.result;
                }
                catch (Exception ex) when (!(ex is RpcException))
                {
                    throw new NetworkException($"Failed to parse RPC response: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// Tests the connection to the RPC endpoint
        /// </summary>
        /// <returns>True if connection is successful</returns>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                // Use chain_get_state_root_hash as a simple test
                await SendRequestAsync<object>("info_get_status", null);
                return true;
            }
            catch (Exception ex)
            {
                if (_enableLogging)
                {
                    Debug.LogWarning($"[CasperSDK] Connection test failed: {ex.Message}");
                }
                return false;
            }
        }
    }
}
