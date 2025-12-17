using System;
using System.Threading.Tasks;
using UnityEngine;
using CasperSDK.Core.Configuration;
using CasperSDK.Core.Interfaces;
using Newtonsoft.Json;

namespace CasperSDK.Services.Block
{
    /// <summary>
    /// Service for querying blockchain blocks.
    /// Provides access to block data and state root hashes.
    /// </summary>
    public class BlockService : IBlockService
    {
        private readonly INetworkClient _networkClient;
        private readonly bool _enableLogging;

        public BlockService(INetworkClient networkClient, NetworkConfig config)
        {
            _networkClient = networkClient ?? throw new ArgumentNullException(nameof(networkClient));
            _enableLogging = config.EnableLogging;
        }

        /// <summary>
        /// Get the latest block from the network.
        /// </summary>
        public async Task<BlockData> GetLatestBlockAsync()
        {
            try
            {
                if (_enableLogging)
                {
                    Debug.Log("[CasperSDK] Getting latest block");
                }

                var result = await _networkClient.SendRequestAsync<BlockResponse>("chain_get_block", null);
                return result?.Block;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CasperSDK] Failed to get latest block: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get a block by its hash.
        /// </summary>
        public async Task<BlockData> GetBlockByHashAsync(string blockHash)
        {
            try
            {
                if (_enableLogging)
                {
                    Debug.Log($"[CasperSDK] Getting block by hash: {blockHash}");
                }

                var param = new BlockIdentifierParam { block_identifier = new BlockHashIdentifier { Hash = blockHash } };
                var result = await _networkClient.SendRequestAsync<BlockResponse>("chain_get_block", param);
                return result?.Block;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CasperSDK] Failed to get block by hash: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get a block by its height.
        /// </summary>
        public async Task<BlockData> GetBlockByHeightAsync(long height)
        {
            try
            {
                if (_enableLogging)
                {
                    Debug.Log($"[CasperSDK] Getting block at height: {height}");
                }

                var param = new BlockIdentifierParam { block_identifier = new BlockHeightIdentifier { Height = height } };
                var result = await _networkClient.SendRequestAsync<BlockResponse>("chain_get_block", param);
                return result?.Block;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CasperSDK] Failed to get block by height: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get the current state root hash.
        /// </summary>
        public async Task<string> GetStateRootHashAsync()
        {
            try
            {
                if (_enableLogging)
                {
                    Debug.Log("[CasperSDK] Getting current state root hash");
                }

                var result = await _networkClient.SendRequestAsync<StateRootHashResponse>("chain_get_state_root_hash", null);
                return result?.StateRootHash;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CasperSDK] Failed to get state root hash: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get state root hash at a specific block height.
        /// </summary>
        public async Task<string> GetStateRootHashAtHeightAsync(long height)
        {
            try
            {
                if (_enableLogging)
                {
                    Debug.Log($"[CasperSDK] Getting state root hash at height: {height}");
                }

                var param = new BlockIdentifierParam { block_identifier = new BlockHeightIdentifier { Height = height } };
                var result = await _networkClient.SendRequestAsync<StateRootHashResponse>("chain_get_state_root_hash", param);
                return result?.StateRootHash;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CasperSDK] Failed to get state root hash at height: {ex.Message}");
                throw;
            }
        }
    }

    #region Response Models

    [Serializable]
    public class BlockResponse
    {
        public string api_version;
        public BlockData block;
        
        public BlockData Block => block;
    }

    [Serializable]
    public class BlockData
    {
        public string hash;
        public BlockHeader header;
        public BlockBody body;
        
        public string Hash => hash;
        public BlockHeader Header => header;
        public BlockBody Body => body;
    }

    [Serializable]
    public class BlockHeader
    {
        public string parent_hash;
        public string state_root_hash;
        public string body_hash;
        public bool random_bit;
        public string accumulated_seed;
        public string era_end;
        public string timestamp;
        public int era_id;
        public long height;
        public string protocol_version;
        
        public string StateRootHash => state_root_hash;
        public long Height => height;
        public string Timestamp => timestamp;
        public int EraId => era_id;
    }

    [Serializable]
    public class BlockBody
    {
        public string proposer;
        public string[] deploy_hashes;
        public string[] transfer_hashes;
        
        public string Proposer => proposer;
        public string[] DeployHashes => deploy_hashes;
        public string[] TransferHashes => transfer_hashes;
    }

    [Serializable]
    public class StateRootHashResponse
    {
        public string api_version;
        public string state_root_hash;
        
        public string StateRootHash => state_root_hash;
    }

    #endregion

    #region Parameter Models

    [Serializable]
    public class BlockIdentifierParam
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object block_identifier;
    }

    [Serializable]
    public class BlockHashIdentifier
    {
        public string Hash;
    }

    [Serializable]
    public class BlockHeightIdentifier
    {
        public long Height;
    }

    #endregion
}
