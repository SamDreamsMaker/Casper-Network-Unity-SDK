using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CasperSDK.Core.Configuration;
using CasperSDK.Core.Interfaces;

namespace CasperSDK.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of INetworkClient for unit testing.
    /// Allows configuring responses and recording calls without network access.
    /// </summary>
    public class MockNetworkClient : INetworkClient
    {
        private readonly Dictionary<string, object> _mockResponses = new Dictionary<string, object>();
        private readonly List<RecordedCall> _recordedCalls = new List<RecordedCall>();
        
        /// <summary>
        /// Exception to throw on the next call (for testing error handling)
        /// </summary>
        public Exception ThrowOnNextCall { get; set; }
        
        /// <summary>
        /// Delay to simulate network latency (milliseconds)
        /// </summary>
        public int SimulatedDelayMs { get; set; } = 0;

        /// <summary>
        /// Gets all recorded RPC calls made to this mock
        /// </summary>
        public IReadOnlyList<RecordedCall> RecordedCalls => _recordedCalls.AsReadOnly();

        /// <inheritdoc/>
        public string Endpoint => "mock://localhost/rpc";

        /// <inheritdoc/>
        public NetworkType NetworkType => NetworkType.Testnet;

        /// <summary>
        /// Configures a mock response for a specific RPC method
        /// </summary>
        public void SetupResponse<TResult>(string method, TResult response)
        {
            _mockResponses[method] = response;
        }

        /// <summary>
        /// Configures a mock response using a factory function for dynamic responses
        /// </summary>
        public void SetupResponseFactory<TResult>(string method, Func<object, TResult> factory)
        {
            _mockResponses[method] = factory;
        }

        /// <summary>
        /// Clears all configured responses
        /// </summary>
        public void ClearResponses()
        {
            _mockResponses.Clear();
        }

        /// <summary>
        /// Clears all recorded calls
        /// </summary>
        public void ClearRecordedCalls()
        {
            _recordedCalls.Clear();
        }

        /// <summary>
        /// Resets the mock to initial state
        /// </summary>
        public void Reset()
        {
            ClearResponses();
            ClearRecordedCalls();
            ThrowOnNextCall = null;
            SimulatedDelayMs = 0;
        }

        /// <summary>
        /// Verifies that a specific method was called
        /// </summary>
        public bool WasCalled(string method)
        {
            return _recordedCalls.Exists(c => c.Method == method);
        }

        /// <summary>
        /// Gets the number of times a method was called
        /// </summary>
        public int GetCallCount(string method)
        {
            return _recordedCalls.FindAll(c => c.Method == method).Count;
        }

        /// <inheritdoc/>
        public async Task<TResult> SendRequestAsync<TResult>(string method, object parameters = null)
        {
            // Record the call
            _recordedCalls.Add(new RecordedCall
            {
                Method = method,
                Parameters = parameters,
                Timestamp = DateTime.UtcNow
            });

            // Simulate network delay if configured
            if (SimulatedDelayMs > 0)
            {
                await Task.Delay(SimulatedDelayMs);
            }

            // Throw exception if configured
            if (ThrowOnNextCall != null)
            {
                var exception = ThrowOnNextCall;
                ThrowOnNextCall = null; // Reset after throwing
                throw exception;
            }

            // Return mock response
            if (_mockResponses.TryGetValue(method, out var response))
            {
                // Check if it's a factory function
                if (response is Func<object, TResult> factory)
                {
                    return factory(parameters);
                }
                
                // Direct response
                if (response is TResult typedResponse)
                {
                    return typedResponse;
                }

                // Try to cast (may throw if types don't match)
                return (TResult)response;
            }

            // No mock configured - return default
            return default;
        }

        /// <inheritdoc/>
        public async Task<bool> TestConnectionAsync()
        {
            await Task.CompletedTask;
            return ThrowOnNextCall == null;
        }
    }

    /// <summary>
    /// Records details of an RPC call for verification in tests
    /// </summary>
    public class RecordedCall
    {
        /// <summary>
        /// The RPC method name
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// The parameters passed to the call
        /// </summary>
        public object Parameters { get; set; }

        /// <summary>
        /// When the call was made
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
