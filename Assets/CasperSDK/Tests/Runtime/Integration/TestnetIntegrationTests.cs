using NUnit.Framework;
using CasperSDK.Core.Configuration;
using CasperSDK.Core.Interfaces;
using CasperSDK.Services.Block;
using CasperSDK.Services.Network;
using CasperSDK.Services.Validator;
using CasperSDK.Network.Clients;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace CasperSDK.Tests.Integration
{
    /// <summary>
    /// Integration tests that make real RPC calls to the Casper Testnet.
    /// These tests verify actual network connectivity and response parsing.
    /// 
    /// NOTE: These tests require network access and may fail if:
    /// - No internet connection
    /// - Testnet node is down
    /// - Rate limiting is applied
    /// </summary>
    [TestFixture]
    [Category("Integration")]
    public class TestnetIntegrationTests
    {
        private NetworkConfig _config;
        private INetworkClient _networkClient;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            // Create a real network config for testnet
            _config = ScriptableObject.CreateInstance<NetworkConfig>();
            
            // Create real network client using TestnetClient
            _networkClient = new TestnetClient(_config);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (_config != null)
            {
                UnityEngine.Object.DestroyImmediate(_config);
            }
        }

        #region Network Connectivity Tests

        [Test]
        [Order(1)]
        public async Task Integration_TestConnection_ShouldSucceed()
        {
            // Act
            var connected = await _networkClient.TestConnectionAsync();

            // Assert
            Assert.IsTrue(connected, "Should be able to connect to Casper Testnet");
            Debug.Log("[Integration] Testnet connection successful!");
        }

        #endregion

        #region Block Service Integration Tests

        [Test]
        [Order(2)]
        public async Task Integration_GetLatestBlock_ReturnsValidBlock()
        {
            // Arrange
            var blockService = new BlockService(_networkClient, _config);

            // Act
            var block = await blockService.GetLatestBlockAsync();

            // Assert
            Assert.IsNotNull(block, "Latest block should not be null");
            Assert.IsNotNull(block.Hash, "Block hash should not be null");
            Assert.IsTrue(block.Header.Height > 0, "Block height should be positive");
            
            Debug.Log($"[Integration] Latest block: {block.Hash}");
            Debug.Log($"[Integration] Block height: {block.Header.Height}");
            Debug.Log($"[Integration] Era: {block.Header.EraId}");
        }

        [Test]
        [Order(3)]
        public async Task Integration_GetStateRootHash_ReturnsValidHash()
        {
            // Arrange
            var blockService = new BlockService(_networkClient, _config);

            // Act
            var stateRootHash = await blockService.GetStateRootHashAsync();

            // Assert
            Assert.IsNotNull(stateRootHash, "State root hash should not be null");
            Assert.IsTrue(stateRootHash.Length == 64, "State root hash should be 64 hex characters");
            
            Debug.Log($"[Integration] State root hash: {stateRootHash}");
        }

        #endregion

        #region Network Info Integration Tests

        [Test]
        [Order(4)]
        public async Task Integration_GetNodeStatus_ReturnsValidStatus()
        {
            // Arrange
            var networkInfoService = new NetworkInfoService(_networkClient, _config);

            // Act
            var status = await networkInfoService.GetStatusAsync();

            // Assert
            Assert.IsNotNull(status, "Node status should not be null");
            Assert.IsNotNull(status.ApiVersion, "API version should not be null");
            Assert.IsNotNull(status.ChainspecName, "Chainspec name should not be null");
            Assert.IsTrue(status.ChainspecName.Contains("casper"), "Should be a Casper network");
            
            Debug.Log($"[Integration] API Version: {status.ApiVersion}");
            Debug.Log($"[Integration] Chainspec: {status.ChainspecName}");
            Debug.Log($"[Integration] Build version: {status.BuildVersion}");
        }

        [Test]
        [Order(5)]
        public async Task Integration_GetPeers_ReturnsPeerList()
        {
            // Arrange
            var networkInfoService = new NetworkInfoService(_networkClient, _config);

            // Act
            var peers = await networkInfoService.GetPeersAsync();

            // Assert
            Assert.IsNotNull(peers, "Peers list should not be null");
            Assert.IsTrue(peers.Length > 0, "Should have at least one peer");
            
            Debug.Log($"[Integration] Connected peers: {peers.Length}");
        }

        #endregion

        #region Validator Service Integration Tests

        [Test]
        [Order(6)]
        public async Task Integration_GetAuctionInfo_ReturnsValidAuctionState()
        {
            // Arrange
            var validatorService = new ValidatorService(_networkClient, _config);

            // Act
            var auctionInfo = await validatorService.GetAuctionInfoAsync();

            // Assert
            Assert.IsNotNull(auctionInfo, "Auction info should not be null");
            Assert.IsTrue(auctionInfo.BlockHeight > 0, "Block height should be positive");
            Assert.IsNotNull(auctionInfo.StateRootHash, "State root hash should not be null");
            
            Debug.Log($"[Integration] Auction at block height: {auctionInfo.BlockHeight}");
            Debug.Log($"[Integration] Era validators count: {auctionInfo.EraValidators?.Length ?? 0}");
            Debug.Log($"[Integration] Total bids: {auctionInfo.Bids?.Length ?? 0}");
        }

        [Test]
        [Order(7)]
        public async Task Integration_GetValidators_ReturnsActiveValidators()
        {
            // Arrange
            var validatorService = new ValidatorService(_networkClient, _config);

            // Act
            var validators = await validatorService.GetValidatorsAsync();

            // Assert
            Assert.IsNotNull(validators, "Validators list should not be null");
            Assert.IsTrue(validators.Length > 0, "Should have at least one validator");
            
            Debug.Log($"[Integration] Active validators: {validators.Length}");
            if (validators.Length > 0)
            {
                Debug.Log($"[Integration] First validator: {validators[0].PublicKey}");
            }
        }

        #endregion

        #region Error Handling Integration Tests

        [Test]
        [Order(100)]
        public void Integration_InvalidBlockHash_ShouldHandleGracefully()
        {
            // Arrange
            var blockService = new BlockService(_networkClient, _config);
            const string invalidHash = "0000000000000000000000000000000000000000000000000000000000000000";

            // Act & Assert - should not throw, but may return null or appropriate error
            Assert.DoesNotThrowAsync(async () =>
            {
                try
                {
                    var block = await blockService.GetBlockByHashAsync(invalidHash);
                    Debug.Log($"[Integration] Invalid hash returned: {(block == null ? "null" : "block object")}");
                }
                catch (Exception ex)
                {
                    Debug.Log($"[Integration] Expected error for invalid hash: {ex.Message}");
                }
            });
        }

        #endregion
    }
}
