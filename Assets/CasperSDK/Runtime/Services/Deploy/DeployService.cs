using System;
using System.Threading.Tasks;
using UnityEngine;
using CasperSDK.Core.Configuration;
using CasperSDK.Core.Interfaces;
using Newtonsoft.Json;

namespace CasperSDK.Services.Deploy
{
    /// <summary>
    /// Service for querying and submitting deploys (transactions).
    /// </summary>
    public class DeployService : IDeployService
    {
        private readonly INetworkClient _networkClient;
        private readonly bool _enableLogging;

        public DeployService(INetworkClient networkClient, NetworkConfig config)
        {
            _networkClient = networkClient ?? throw new ArgumentNullException(nameof(networkClient));
            _enableLogging = config.EnableLogging;
        }

        /// <summary>
        /// Get a deploy by its hash.
        /// </summary>
        public async Task<DeployInfo> GetDeployAsync(string deployHash)
        {
            try
            {
                if (string.IsNullOrEmpty(deployHash))
                {
                    throw new ArgumentException("Deploy hash cannot be null or empty", nameof(deployHash));
                }

                if (_enableLogging)
                {
                    Debug.Log($"[CasperSDK] Getting deploy: {deployHash}");
                }

                var param = new DeployHashParam { deploy_hash = deployHash };
                var result = await _networkClient.SendRequestAsync<DeployResponse>("info_get_deploy", param);

                if (result?.deploy == null)
                {
                    Debug.LogWarning($"[CasperSDK] Deploy not found: {deployHash}");
                    return null;
                }

                return new DeployInfo
                {
                    Hash = result.deploy.hash,
                    Header = result.deploy.header,
                    ExecutionResults = result.execution_results
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CasperSDK] Failed to get deploy: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get deploy execution status.
        /// </summary>
        public async Task<DeployExecutionStatus> GetDeployStatusAsync(string deployHash)
        {
            try
            {
                var deploy = await GetDeployAsync(deployHash);

                if (deploy == null)
                {
                    return new DeployExecutionStatus
                    {
                        DeployHash = deployHash,
                        Status = ExecutionStatus.NotFound
                    };
                }

                if (deploy.ExecutionResults == null || deploy.ExecutionResults.Length == 0)
                {
                    return new DeployExecutionStatus
                    {
                        DeployHash = deployHash,
                        Status = ExecutionStatus.Pending
                    };
                }

                var lastResult = deploy.ExecutionResults[deploy.ExecutionResults.Length - 1];
                bool success = lastResult.result?.Success != null;

                return new DeployExecutionStatus
                {
                    DeployHash = deployHash,
                    Status = success ? ExecutionStatus.Success : ExecutionStatus.Failed,
                    BlockHash = lastResult.block_hash,
                    Cost = lastResult.result?.Success?.cost ?? lastResult.result?.Failure?.cost,
                    ErrorMessage = lastResult.result?.Failure?.error_message
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CasperSDK] Failed to get deploy status: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Submit a signed deploy to the network.
        /// Note: The deploy must be properly signed before submission.
        /// </summary>
        public async Task<string> SubmitDeployAsync(object signedDeploy)
        {
            try
            {
                if (signedDeploy == null)
                {
                    throw new ArgumentNullException(nameof(signedDeploy));
                }

                if (_enableLogging)
                {
                    Debug.Log("[CasperSDK] Submitting deploy to network");
                }

                var param = new DeploySubmitParam { deploy = signedDeploy };
                var result = await _networkClient.SendRequestAsync<DeploySubmitResponse>("account_put_deploy", param);

                if (string.IsNullOrEmpty(result?.deploy_hash))
                {
                    throw new Exception("Deploy submission failed - no deploy hash returned");
                }

                if (_enableLogging)
                {
                    Debug.Log($"[CasperSDK] Deploy submitted successfully: {result.deploy_hash}");
                }

                return result.deploy_hash;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CasperSDK] Failed to submit deploy: {ex.Message}");
                throw;
            }
        }
    }

    #region Enums

    public enum ExecutionStatus
    {
        NotFound,
        Pending,
        Success,
        Failed
    }

    #endregion

    #region Response Models

    [Serializable]
    public class DeployResponse
    {
        public string api_version;
        public DeployData deploy;
        public ExecutionResultWrapper[] execution_results;
    }

    [Serializable]
    public class DeployData
    {
        public string hash;
        public DeployHeader header;
        public DeployPayload payment;
        public DeployPayload session;
        public ApprovalData[] approvals;
    }

    [Serializable]
    public class DeployHeader
    {
        public string account;
        public string timestamp;
        public string ttl;
        public long gas_price;
        public string body_hash;
        public string[] dependencies;
        public string chain_name;
    }

    [Serializable]
    public class DeployPayload
    {
        public object ModuleBytes;
        public object StoredContractByHash;
        public object StoredContractByName;
        public object StoredVersionedContractByHash;
        public object StoredVersionedContractByName;
        public object Transfer;
    }

    [Serializable]
    public class ApprovalData
    {
        public string signer;
        public string signature;
    }

    [Serializable]
    public class ExecutionResultWrapper
    {
        public string block_hash;
        public ExecutionResult result;
    }

    [Serializable]
    public class ExecutionResult
    {
        public SuccessResult Success;
        public FailureResult Failure;
    }

    [Serializable]
    public class SuccessResult
    {
        public object[] effect;
        public object[] transfers;
        public string cost;
    }

    [Serializable]
    public class FailureResult
    {
        public string error_message;
        public string cost;
    }

    [Serializable]
    public class DeploySubmitResponse
    {
        public string api_version;
        public string deploy_hash;
    }

    #endregion

    #region Parameter Models

    [Serializable]
    public class DeployHashParam
    {
        public string deploy_hash;
    }

    [Serializable]
    public class DeploySubmitParam
    {
        public object deploy;
    }

    #endregion

    #region Public Models

    public class DeployInfo
    {
        public string Hash { get; set; }
        public DeployHeader Header { get; set; }
        public ExecutionResultWrapper[] ExecutionResults { get; set; }
    }

    public class DeployExecutionStatus
    {
        public string DeployHash { get; set; }
        public ExecutionStatus Status { get; set; }
        public string BlockHash { get; set; }
        public string Cost { get; set; }
        public string ErrorMessage { get; set; }
    }

    #endregion
}
