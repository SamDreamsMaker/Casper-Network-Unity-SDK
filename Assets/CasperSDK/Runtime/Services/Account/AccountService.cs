using System;
using System.Threading.Tasks;
using UnityEngine;
using CasperSDK.Core.Configuration;
using CasperSDK.Core.Interfaces;
using CasperSDK.Models;
using CasperSDK.Unity;
using Newtonsoft.Json;

namespace CasperSDK.Services.Account
{
    /// <summary>
    /// Service for account management operations.
    /// Implements Repository pattern for account data access.
    /// </summary>
    public class AccountService : IAccountService
    {
        private readonly INetworkClient _networkClient;
        private readonly NetworkConfig _config;
        private readonly bool _enableLogging;

        /// <summary>
        /// Initializes a new instance of AccountService
        /// </summary>
        /// <param name="networkClient">Network client for RPC calls</param>
        /// <param name="config">Network configuration</param>
        public AccountService(INetworkClient networkClient, NetworkConfig config)
        {
            _networkClient = networkClient ?? throw new ArgumentNullException(nameof(networkClient));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _enableLogging = config.EnableLogging;
        }

        /// <inheritdoc/>
        public async Task<Models.Account> GetAccountAsync(string publicKey)
        {
            if (string.IsNullOrWhiteSpace(publicKey))
            {
                throw new ArgumentNullException(nameof(publicKey));
            }

            try
            {
                if (_enableLogging)
                {
                    Debug.Log($"[CasperSDK] Getting account info for: {publicKey}");
                }

                // Get account info via RPC
                var parameters = new { public_key = publicKey };
                var result = await _networkClient.SendRequestAsync<object>("state_get_account_info", parameters);

                // For now, return a basic account structure
                // TODO: Properly parse the response once we have the actual response structure
                var account = new Models.Account
                {
                    PublicKey = publicKey,
                    AccountHash = $"account-hash-{publicKey.Substring(0, 8)}..."
                };

                if (_enableLogging)
                {
                    Debug.Log($"[CasperSDK] Account retrieved successfully");
                }

                return account;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CasperSDK] Failed to get account: {ex.Message}");
                throw new CasperSDKException("Failed to retrieve account information", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetBalanceAsync(string publicKey)
        {
            if (string.IsNullOrWhiteSpace(publicKey))
            {
                throw new ArgumentNullException(nameof(publicKey));
            }

            try
            {
                if (_enableLogging)
                {
                    Debug.Log($"[CasperSDK] Getting balance for: {publicKey}");
                }

                // Get current state root hash - required for state_get_balance
                var statusResponse = await _networkClient.SendRequestAsync<StatusResponse>("info_get_status", null);
                string stateRootHash = statusResponse?.LastAddedBlockInfo?.StateRootHash;
                
                if (string.IsNullOrEmpty(stateRootHash))
                {
                    throw new CasperSDKException("Failed to get current state root hash");
                }

                // Get account info to obtain main purse
                var accountParams = new AccountInfoParams { public_key = publicKey };
                var accountResult = await _networkClient.SendRequestAsync<AccountInfoRpcResponse>("state_get_account_info", accountParams);

                if (accountResult?.Account?.MainPurse == null)
                {
                    throw new CasperSDKException("Failed to retrieve account main purse");
                }

                string mainPurse = accountResult.Account.MainPurse;

                if (_enableLogging)
                {
                    Debug.Log($"[CasperSDK] Main purse URef: {mainPurse}");
                }

                // Get balance from purse with state_root_hash
                var balanceParams = new BalanceParams 
                { 
                    purse_uref = mainPurse,
                    state_root_hash = stateRootHash
                };
                
                var balanceResult = await _networkClient.SendRequestAsync<BalanceRpcResponse>("state_get_balance", balanceParams);

                string balance = balanceResult?.BalanceValue ?? "0";

                if (_enableLogging)
                {
                    Debug.Log($"[CasperSDK] Balance: {balance} motes");
                }

                return balance;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CasperSDK] Failed to get balance: {ex.Message}");
                throw new CasperSDKException("Failed to retrieve account balance", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<KeyPair> GenerateKeyPairAsync(KeyAlgorithm algorithm = KeyAlgorithm.ED25519)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (_enableLogging)
                    {
                        Debug.Log($"[CasperSDK] Generating {algorithm} key pair");
                    }

                    // TODO: Implement actual key generation using Casper.Network.SDK
                    // For now, return a placeholder
                    var keyPair = new KeyPair
                    {
                        Algorithm = algorithm,
                        PublicKeyHex = "01" + GenerateRandomHex(64),
                        PrivateKeyHex = GenerateRandomHex(64),
                        AccountHash = "account-hash-" + GenerateRandomHex(64)
                    };

                    if (_enableLogging)
                    {
                        Debug.Log($"[CasperSDK] Key pair generated successfully");
                    }

                    return keyPair;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[CasperSDK] Failed to generate key pair: {ex.Message}");
                    throw new CasperSDKException("Failed to generate key pair", ex);
                }
            });
        }

        /// <inheritdoc/>
        public async Task<KeyPair> ImportAccountAsync(string privateKeyHex, KeyAlgorithm algorithm = KeyAlgorithm.ED25519)
        {
            if (string.IsNullOrWhiteSpace(privateKeyHex))
            {
                throw new ArgumentNullException(nameof(privateKeyHex));
            }

            return await Task.Run(() =>
            {
                try
                {
                    if (_enableLogging)
                    {
                        Debug.Log($"[CasperSDK] Importing account with {algorithm} private key");
                    }

                    // TODO: Implement actual key import using Casper.Network.SDK
                    // Derive public key from private key
                    var keyPair = new KeyPair
                    {
                        Algorithm = algorithm,
                        PrivateKeyHex = privateKeyHex,
                        PublicKeyHex = "01" + GenerateRandomHex(64), // Placeholder
                        AccountHash = "account-hash-" + GenerateRandomHex(64)
                    };

                    if (_enableLogging)
                    {
                        Debug.Log($"[CasperSDK] Account imported successfully");
                    }

                    return keyPair;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[CasperSDK] Failed to import account: {ex.Message}");
                    throw new CasperSDKException("Failed to import account", ex);
                }
            });
        }

        private string GenerateRandomHex(int length)
        {
            var random = new System.Random();
            var bytes = new byte[length / 2];
            random.NextBytes(bytes);
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }

    // Response models for JSON RPC parsing
    // These models match the Casper RPC response structure
    [Serializable]
    public class AccountInfoRpcResponse
    {
        public string api_version;
        public AccountDataResponse account;

        public AccountDataResponse Account => account;
    }

    [Serializable]
    public class AccountDataResponse
    {
        public string main_purse;

        public string MainPurse => main_purse;
    }

    [Serializable]
    public class BalanceRpcResponse
    {
        public string api_version;
        public string balance_value;

        public string BalanceValue => balance_value;
    }

    // RPC Parameter classes - simple format
    [Serializable]
    public class AccountInfoParams
    {
        public string public_key;
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public BlockIdentifier block_identifier; // Optional
    }

    [Serializable]
    public class BlockIdentifier
    {
        public int Height;
    }

    [Serializable]
    public class BalanceParams
    {
        public string purse_uref;
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string state_root_hash; // Optional
    }

    // Response for info_get_status
    [Serializable]
    public class StatusResponse
    {
        public string api_version;
        public BlockInfo last_added_block_info;

        public BlockInfo LastAddedBlockInfo => last_added_block_info;
    }

    [Serializable]
    public class BlockInfo
    {
        public string hash;
        public string state_root_hash;
        public int height;

        public string StateRootHash => state_root_hash;
    }
}
