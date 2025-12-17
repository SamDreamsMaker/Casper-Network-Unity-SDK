using NUnit.Framework;
using CasperSDK.Core.Configuration;
using CasperSDK.Services.Block;
using CasperSDK.Tests.Mocks;
using System;
using System.Threading.Tasks;

namespace CasperSDK.Tests.Unit
{
    /// <summary>
    /// Unit tests for BlockService
    /// Tests block queries and state root hash retrieval
    /// </summary>
    [TestFixture]
    public class BlockServiceTests
    {
        private MockNetworkClient _mockClient;
        private NetworkConfig _config;
        private BlockService _blockService;

        [SetUp]
        public void Setup()
        {
            _mockClient = new MockNetworkClient();
            _config = UnityEngine.ScriptableObject.CreateInstance<NetworkConfig>();
            _config.EnableLogging = false;
            _blockService = new BlockService(_mockClient, _config);
        }

        [TearDown]
        public void TearDown()
        {
            _mockClient.Reset();
            if (_config != null)
            {
                UnityEngine.Object.DestroyImmediate(_config);
            }
        }

        #region GetLatestBlockAsync Tests

        [Test]
        public async Task GetLatestBlockAsync_ReturnsBlock()
        {
            // Arrange
            _mockClient.SetupResponse("chain_get_block", new TestBlockResponse
            {
                api_version = "2.0.0",
                block = new TestBlockData
                {
                    hash = "block-hash-abc123",
                    header = new TestBlockHeader
                    {
                        height = 12345,
                        era_id = 100,
                        state_root_hash = "state-root-xyz",
                        timestamp = "2024-01-01T00:00:00Z"
                    },
                    body = new TestBlockBody
                    {
                        proposer = "01proposer123"
                    }
                }
            });

            // Act
            var block = await _blockService.GetLatestBlockAsync();

            // Assert
            Assert.IsNotNull(block);
            Assert.AreEqual("block-hash-abc123", block.Hash);
            Assert.AreEqual(12345, block.Header.Height);
            Assert.AreEqual(100, block.Header.EraId);
            Assert.IsTrue(_mockClient.WasCalled("chain_get_block"));
        }

        [Test]
        public async Task GetLatestBlockAsync_NoBlockReturned_ReturnsNull()
        {
            // Arrange - no mock response configured, returns null

            // Act
            var block = await _blockService.GetLatestBlockAsync();

            // Assert
            Assert.IsNull(block);
        }

        #endregion

        #region GetBlockByHashAsync Tests

        [Test]
        public async Task GetBlockByHashAsync_ValidHash_ReturnsBlock()
        {
            // Arrange
            const string blockHash = "abc123def456";
            
            _mockClient.SetupResponse("chain_get_block", new TestBlockResponse
            {
                block = new TestBlockData
                {
                    hash = blockHash,
                    header = new TestBlockHeader { height = 999 }
                }
            });

            // Act
            var block = await _blockService.GetBlockByHashAsync(blockHash);

            // Assert
            Assert.IsNotNull(block);
            Assert.AreEqual(blockHash, block.Hash);
            Assert.AreEqual(1, _mockClient.GetCallCount("chain_get_block"));
        }

        [Test]
        public void GetBlockByHashAsync_NetworkError_ThrowsException()
        {
            // Arrange
            _mockClient.ThrowOnNextCall = new Exception("Block not found");

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () =>
            {
                await _blockService.GetBlockByHashAsync("invalid-hash");
            });
        }

        #endregion

        #region GetBlockByHeightAsync Tests

        [Test]
        public async Task GetBlockByHeightAsync_ValidHeight_ReturnsBlock()
        {
            // Arrange
            const long height = 50000;
            
            _mockClient.SetupResponse("chain_get_block", new TestBlockResponse
            {
                block = new TestBlockData
                {
                    hash = "block-at-height",
                    header = new TestBlockHeader { height = height }
                }
            });

            // Act
            var block = await _blockService.GetBlockByHeightAsync(height);

            // Assert
            Assert.IsNotNull(block);
            Assert.AreEqual(height, block.Header.Height);
        }

        #endregion

        #region GetStateRootHashAsync Tests

        [Test]
        public async Task GetStateRootHashAsync_ReturnsHash()
        {
            // Arrange
            const string expectedHash = "state-root-hash-xyz789";
            
            _mockClient.SetupResponse("chain_get_state_root_hash", new TestStateRootHashResponse
            {
                api_version = "2.0.0",
                state_root_hash = expectedHash
            });

            // Act
            var hash = await _blockService.GetStateRootHashAsync();

            // Assert
            Assert.IsNotNull(hash);
            Assert.AreEqual(expectedHash, hash);
            Assert.IsTrue(_mockClient.WasCalled("chain_get_state_root_hash"));
        }

        [Test]
        public async Task GetStateRootHashAtHeightAsync_ValidHeight_ReturnsHash()
        {
            // Arrange
            const long height = 1000;
            const string expectedHash = "state-root-at-height";
            
            _mockClient.SetupResponse("chain_get_state_root_hash", new TestStateRootHashResponse
            {
                state_root_hash = expectedHash
            });

            // Act
            var hash = await _blockService.GetStateRootHashAtHeightAsync(height);

            // Assert
            Assert.IsNotNull(hash);
            Assert.AreEqual(expectedHash, hash);
        }

        #endregion
    }
}
