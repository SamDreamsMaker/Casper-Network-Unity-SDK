using System;
using System.Threading.Tasks;
using UnityEngine;
using CasperSDK.Core.Configuration;
using CasperSDK.Core.Interfaces;
using Newtonsoft.Json;

namespace CasperSDK.Services.Network
{
    /// <summary>
    /// Service for querying network status and peer information.
    /// </summary>
    public class NetworkInfoService : INetworkInfoService
    {
        private readonly INetworkClient _networkClient;
        private readonly bool _enableLogging;

        public NetworkInfoService(INetworkClient networkClient, NetworkConfig config)
        {
            _networkClient = networkClient ?? throw new ArgumentNullException(nameof(networkClient));
            _enableLogging = config.EnableLogging;
        }

        /// <summary>
        /// Get the current node status.
        /// </summary>
        public async Task<NodeStatus> GetStatusAsync()
        {
            try
            {
                if (_enableLogging)
                {
                    Debug.Log("[CasperSDK] Getting node status");
                }

                var result = await _networkClient.SendRequestAsync<StatusRpcResponse>("info_get_status", null);
                return new NodeStatus
                {
                    ApiVersion = result?.api_version,
                    ChainspecName = result?.chainspec_name,
                    StartingStateRootHash = result?.starting_state_root_hash,
                    Peers = result?.peers,
                    LastAddedBlockInfo = result?.last_added_block_info,
                    BuildVersion = result?.build_version,
                    Uptime = result?.uptime
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CasperSDK] Failed to get node status: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get list of connected peers.
        /// </summary>
        public async Task<PeerInfo[]> GetPeersAsync()
        {
            try
            {
                if (_enableLogging)
                {
                    Debug.Log("[CasperSDK] Getting network peers");
                }

                var result = await _networkClient.SendRequestAsync<PeersResponse>("info_get_peers", null);
                return result?.peers;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CasperSDK] Failed to get peers: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get the chain specification.
        /// </summary>
        public async Task<ChainspecInfo> GetChainspecAsync()
        {
            try
            {
                if (_enableLogging)
                {
                    Debug.Log("[CasperSDK] Getting chainspec");
                }

                var result = await _networkClient.SendRequestAsync<ChainspecResponse>("info_get_chainspec", null);
                return new ChainspecInfo
                {
                    ApiVersion = result?.api_version,
                    ChainspecBytes = result?.chainspec_bytes
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CasperSDK] Failed to get chainspec: {ex.Message}");
                throw;
            }
        }
    }

    #region Response Models

    [Serializable]
    public class StatusRpcResponse
    {
        public string api_version;
        public string chainspec_name;
        public string starting_state_root_hash;
        public PeerInfo[] peers;
        public BlockInfoResponse last_added_block_info;
        public string build_version;
        public string uptime;
    }

    [Serializable]
    public class BlockInfoResponse
    {
        public string hash;
        public string timestamp;
        public int era_id;
        public long height;
        public string state_root_hash;
        public string creator;
    }

    [Serializable]
    public class PeersResponse
    {
        public string api_version;
        public PeerInfo[] peers;
    }

    [Serializable]
    public class PeerInfo
    {
        public string node_id;
        public string address;
        
        public string NodeId => node_id;
        public string Address => address;
    }

    [Serializable]
    public class ChainspecResponse
    {
        public string api_version;
        public ChainspecBytesData chainspec_bytes;
    }

    [Serializable]
    public class ChainspecBytesData
    {
        public string chainspec_bytes;
        public string maybe_genesis_accounts_bytes;
        public string maybe_global_state_bytes;
    }

    #endregion

    #region Public Models

    public class NodeStatus
    {
        public string ApiVersion { get; set; }
        public string ChainspecName { get; set; }
        public string StartingStateRootHash { get; set; }
        public PeerInfo[] Peers { get; set; }
        public BlockInfoResponse LastAddedBlockInfo { get; set; }
        public string BuildVersion { get; set; }
        public string Uptime { get; set; }
    }

    public class ChainspecInfo
    {
        public string ApiVersion { get; set; }
        public ChainspecBytesData ChainspecBytes { get; set; }
    }

    #endregion
}
