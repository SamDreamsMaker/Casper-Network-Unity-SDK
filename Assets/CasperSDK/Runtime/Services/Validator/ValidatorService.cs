using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using CasperSDK.Core.Configuration;
using CasperSDK.Core.Interfaces;
using Newtonsoft.Json;

namespace CasperSDK.Services.Validator
{
    /// <summary>
    /// Service for querying validator and staking information.
    /// </summary>
    public class ValidatorService : IValidatorService
    {
        private readonly INetworkClient _networkClient;
        private readonly bool _enableLogging;

        public ValidatorService(INetworkClient networkClient, NetworkConfig config)
        {
            _networkClient = networkClient ?? throw new ArgumentNullException(nameof(networkClient));
            _enableLogging = config.EnableLogging;
        }

        /// <summary>
        /// Get auction information including all bids and validators.
        /// </summary>
        public async Task<AuctionInfo> GetAuctionInfoAsync(string blockHash = null)
        {
            try
            {
                if (_enableLogging)
                {
                    Debug.Log("[CasperSDK] Getting auction info");
                }

                object param = null;
                if (!string.IsNullOrEmpty(blockHash))
                {
                    param = new { block_identifier = new { Hash = blockHash } };
                }

                var result = await _networkClient.SendRequestAsync<AuctionInfoResponse>("state_get_auction_info", param);

                if (result?.auction_state == null)
                {
                    Debug.LogWarning("[CasperSDK] No auction info returned");
                    return null;
                }

                return new AuctionInfo
                {
                    StateRootHash = result.auction_state.state_root_hash,
                    BlockHeight = result.auction_state.block_height,
                    EraValidators = result.auction_state.era_validators,
                    Bids = result.auction_state.bids
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CasperSDK] Failed to get auction info: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get list of active validators for the current era.
        /// </summary>
        public async Task<ValidatorInfo[]> GetValidatorsAsync()
        {
            try
            {
                if (_enableLogging)
                {
                    Debug.Log("[CasperSDK] Getting current validators");
                }

                var auctionInfo = await GetAuctionInfoAsync();
                
                if (auctionInfo?.EraValidators == null || auctionInfo.EraValidators.Length == 0)
                {
                    return new ValidatorInfo[0];
                }

                // Get validators from the most recent era
                var currentEra = auctionInfo.EraValidators[auctionInfo.EraValidators.Length - 1];
                var validators = new List<ValidatorInfo>();

                if (currentEra?.validator_weights != null)
                {
                    foreach (var weight in currentEra.validator_weights)
                    {
                        validators.Add(new ValidatorInfo
                        {
                            PublicKey = weight.public_key,
                            Weight = weight.weight,
                            EraId = currentEra.era_id
                        });
                    }
                }

                if (_enableLogging)
                {
                    Debug.Log($"[CasperSDK] Found {validators.Count} validators");
                }

                return validators.ToArray();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CasperSDK] Failed to get validators: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get validator info by public key.
        /// </summary>
        public async Task<ValidatorBid> GetValidatorByKeyAsync(string publicKey)
        {
            try
            {
                if (string.IsNullOrEmpty(publicKey))
                {
                    throw new ArgumentException("Public key cannot be null or empty", nameof(publicKey));
                }

                if (_enableLogging)
                {
                    Debug.Log($"[CasperSDK] Getting validator info for: {publicKey}");
                }

                var auctionInfo = await GetAuctionInfoAsync();
                
                if (auctionInfo?.Bids == null)
                {
                    return null;
                }

                foreach (var bid in auctionInfo.Bids)
                {
                    if (bid.public_key?.Equals(publicKey, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        return new ValidatorBid
                        {
                            PublicKey = bid.public_key,
                            BondingPurse = bid.bid?.bonding_purse,
                            StakedAmount = bid.bid?.staked_amount,
                            DelegationRate = bid.bid?.delegation_rate ?? 0,
                            Inactive = bid.bid?.inactive ?? false
                        };
                    }
                }

                Debug.LogWarning($"[CasperSDK] Validator not found: {publicKey}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CasperSDK] Failed to get validator: {ex.Message}");
                throw;
            }
        }
    }

    #region Response Models

    [Serializable]
    public class AuctionInfoResponse
    {
        public string api_version;
        public AuctionState auction_state;
    }

    [Serializable]
    public class AuctionState
    {
        public string state_root_hash;
        public long block_height;
        public EraValidator[] era_validators;
        public BidInfo[] bids;
    }

    [Serializable]
    public class EraValidator
    {
        public int era_id;
        public ValidatorWeight[] validator_weights;
    }

    [Serializable]
    public class ValidatorWeight
    {
        public string public_key;
        public string weight;
    }

    [Serializable]
    public class BidInfo
    {
        public string public_key;
        public BidData bid;
    }

    [Serializable]
    public class BidData
    {
        public string bonding_purse;
        public string staked_amount;
        public int delegation_rate;
        public bool inactive;
        public DelegatorInfo[] delegators;
    }

    [Serializable]
    public class DelegatorInfo
    {
        public string delegator_public_key;
        public string staked_amount;
        public string bonding_purse;
    }

    #endregion

    #region Public Models

    public class AuctionInfo
    {
        public string StateRootHash { get; set; }
        public long BlockHeight { get; set; }
        public EraValidator[] EraValidators { get; set; }
        public BidInfo[] Bids { get; set; }
    }

    public class ValidatorInfo
    {
        public string PublicKey { get; set; }
        public string Weight { get; set; }
        public int EraId { get; set; }
    }

    public class ValidatorBid
    {
        public string PublicKey { get; set; }
        public string BondingPurse { get; set; }
        public string StakedAmount { get; set; }
        public int DelegationRate { get; set; }
        public bool Inactive { get; set; }
    }

    #endregion
}
