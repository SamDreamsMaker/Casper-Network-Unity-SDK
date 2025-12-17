using System;
using System.Threading.Tasks;
using UnityEngine;
using CasperSDK.Core.Configuration;
using CasperSDK.Core.Interfaces;
using Newtonsoft.Json;

namespace CasperSDK.Services.State
{
    /// <summary>
    /// Service for querying global state and dictionary items.
    /// </summary>
    public class StateService : IStateService
    {
        private readonly INetworkClient _networkClient;
        private readonly bool _enableLogging;

        public StateService(INetworkClient networkClient, NetworkConfig config)
        {
            _networkClient = networkClient ?? throw new ArgumentNullException(nameof(networkClient));
            _enableLogging = config.EnableLogging;
        }

        /// <summary>
        /// Query global state by key.
        /// </summary>
        public async Task<GlobalStateValue> QueryGlobalStateAsync(string key, string stateRootHash = null)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentException("Key cannot be null or empty", nameof(key));
                }

                if (_enableLogging)
                {
                    Debug.Log($"[CasperSDK] Querying global state for key: {key}");
                }

                var param = new QueryGlobalStateParams
                {
                    key = key,
                    path = new string[0]
                };

                // Use state_root_hash if provided, otherwise query by block
                if (!string.IsNullOrEmpty(stateRootHash))
                {
                    param.state_identifier = new StateIdentifier { StateRootHash = stateRootHash };
                }

                var result = await _networkClient.SendRequestAsync<QueryGlobalStateResponse>("query_global_state", param);

                if (result?.stored_value == null)
                {
                    Debug.LogWarning($"[CasperSDK] No value found for key: {key}");
                    return null;
                }

                return new GlobalStateValue
                {
                    Key = key,
                    StoredValue = result.stored_value,
                    MerkleProof = result.merkle_proof
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CasperSDK] Failed to query global state: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get a dictionary item by its key.
        /// </summary>
        public async Task<DictionaryItem> GetDictionaryItemAsync(string dictionaryKey, string seedUref, string stateRootHash = null)
        {
            try
            {
                if (string.IsNullOrEmpty(dictionaryKey))
                {
                    throw new ArgumentException("Dictionary key cannot be null or empty", nameof(dictionaryKey));
                }

                if (string.IsNullOrEmpty(seedUref))
                {
                    throw new ArgumentException("Seed URef cannot be null or empty", nameof(seedUref));
                }

                if (_enableLogging)
                {
                    Debug.Log($"[CasperSDK] Getting dictionary item: {dictionaryKey}");
                }

                var param = new DictionaryItemParams
                {
                    dictionary_identifier = new DictionaryIdentifier
                    {
                        URef = new URefDictionaryIdentifier
                        {
                            seed_uref = seedUref,
                            dictionary_item_key = dictionaryKey
                        }
                    }
                };

                if (!string.IsNullOrEmpty(stateRootHash))
                {
                    param.state_root_hash = stateRootHash;
                }

                var result = await _networkClient.SendRequestAsync<DictionaryItemResponse>("state_get_dictionary_item", param);

                if (result?.stored_value == null)
                {
                    Debug.LogWarning($"[CasperSDK] Dictionary item not found: {dictionaryKey}");
                    return null;
                }

                return new DictionaryItem
                {
                    Key = dictionaryKey,
                    DictionaryKey = result.dictionary_key,
                    StoredValue = result.stored_value,
                    MerkleProof = result.merkle_proof
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CasperSDK] Failed to get dictionary item: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get a dictionary item by contract named key.
        /// </summary>
        public async Task<DictionaryItem> GetDictionaryItemByNameAsync(string contractHash, string dictionaryName, string dictionaryKey, string stateRootHash = null)
        {
            try
            {
                if (string.IsNullOrEmpty(contractHash))
                {
                    throw new ArgumentException("Contract hash cannot be null or empty", nameof(contractHash));
                }

                if (_enableLogging)
                {
                    Debug.Log($"[CasperSDK] Getting dictionary item by name: {dictionaryName}/{dictionaryKey}");
                }

                var param = new DictionaryItemParams
                {
                    dictionary_identifier = new DictionaryIdentifier
                    {
                        ContractNamedKey = new ContractNamedKeyIdentifier
                        {
                            key = contractHash,
                            dictionary_name = dictionaryName,
                            dictionary_item_key = dictionaryKey
                        }
                    }
                };

                if (!string.IsNullOrEmpty(stateRootHash))
                {
                    param.state_root_hash = stateRootHash;
                }

                var result = await _networkClient.SendRequestAsync<DictionaryItemResponse>("state_get_dictionary_item", param);

                if (result?.stored_value == null)
                {
                    return null;
                }

                return new DictionaryItem
                {
                    Key = dictionaryKey,
                    DictionaryKey = result.dictionary_key,
                    StoredValue = result.stored_value,
                    MerkleProof = result.merkle_proof
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CasperSDK] Failed to get dictionary item by name: {ex.Message}");
                throw;
            }
        }
    }

    #region Response Models

    [Serializable]
    public class QueryGlobalStateResponse
    {
        public string api_version;
        public string block_header;
        public object stored_value;
        public string merkle_proof;
    }

    [Serializable]
    public class DictionaryItemResponse
    {
        public string api_version;
        public string dictionary_key;
        public object stored_value;
        public string merkle_proof;
    }

    #endregion

    #region Parameter Models

    [Serializable]
    public class QueryGlobalStateParams
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public StateIdentifier state_identifier;
        public string key;
        public string[] path;
    }

    [Serializable]
    public class StateIdentifier
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string StateRootHash;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BlockHash;
    }

    [Serializable]
    public class DictionaryItemParams
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string state_root_hash;
        public DictionaryIdentifier dictionary_identifier;
    }

    [Serializable]
    public class DictionaryIdentifier
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public URefDictionaryIdentifier URef;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ContractNamedKeyIdentifier ContractNamedKey;
    }

    [Serializable]
    public class URefDictionaryIdentifier
    {
        public string seed_uref;
        public string dictionary_item_key;
    }

    [Serializable]
    public class ContractNamedKeyIdentifier
    {
        public string key;
        public string dictionary_name;
        public string dictionary_item_key;
    }

    #endregion

    #region Public Models

    public class GlobalStateValue
    {
        public string Key { get; set; }
        public object StoredValue { get; set; }
        public string MerkleProof { get; set; }
    }

    public class DictionaryItem
    {
        public string Key { get; set; }
        public string DictionaryKey { get; set; }
        public object StoredValue { get; set; }
        public string MerkleProof { get; set; }
    }

    #endregion
}
