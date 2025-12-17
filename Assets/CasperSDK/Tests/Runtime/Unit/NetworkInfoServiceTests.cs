using NUnit.Framework;
using CasperSDK.Core.Configuration;
using CasperSDK.Services.Network;
using CasperSDK.Tests.Mocks;
using System;
using System.Threading.Tasks;

namespace CasperSDK.Tests.Unit
{
    /// <summary>
    /// Unit tests for NetworkInfoService
    /// Tests node status, peers, and chainspec queries
    /// </summary>
    [TestFixture]
    public class NetworkInfoServiceTests
    {
        private MockNetworkClient _mockClient;
        private NetworkConfig _config;
        private NetworkInfoService _networkInfoService;

        [SetUp]
        public void Setup()
        {
            _mockClient = new MockNetworkClient();
            _config = UnityEngine.ScriptableObject.CreateInstance<NetworkConfig>();
            _config.EnableLogging = false;
            _networkInfoService = new NetworkInfoService(_mockClient, _config);
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

        #region GetStatusAsync Tests

        [Test]
        public async Task GetStatusAsync_ReturnsNodeStatus()
        {
            // Arrange
            _mockClient.SetupResponse("info_get_status", new StatusRpcResponse
            {
                api_version = "2.0.0",
                chainspec_name = "casper-test",
                starting_state_root_hash = "state-root-abc",
                build_version = "1.5.0",
                uptime = "24h 30m",
                peers = new[]
                {
                    new PeerInfo { node_id = "peer1", address = "192.168.1.1:35000" },
                    new PeerInfo { node_id = "peer2", address = "192.168.1.2:35000" }
                },
                last_added_block_info = new BlockInfoResponse
                {
                    hash = "block-hash-xyz",
                    height = 12345,
                    era_id = 100
                }
            });

            // Act
            var status = await _networkInfoService.GetStatusAsync();

            // Assert
            Assert.IsNotNull(status);
            Assert.AreEqual("2.0.0", status.ApiVersion);
            Assert.AreEqual("casper-test", status.ChainspecName);
            Assert.IsNotNull(status.Peers);
            Assert.AreEqual(2, status.Peers.Length);
            Assert.IsNotNull(status.LastAddedBlockInfo);
            Assert.AreEqual(12345, status.LastAddedBlockInfo.height);
            Assert.IsTrue(_mockClient.WasCalled("info_get_status"));
        }

        [Test]
        public void GetStatusAsync_NetworkError_ThrowsException()
        {
            // Arrange
            _mockClient.ThrowOnNextCall = new Exception("Connection refused");

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () =>
            {
                await _networkInfoService.GetStatusAsync();
            });
        }

        #endregion

        #region GetPeersAsync Tests

        [Test]
        public async Task GetPeersAsync_ReturnsPeerList()
        {
            // Arrange
            _mockClient.SetupResponse("info_get_peers", new PeersResponse
            {
                api_version = "2.0.0",
                peers = new[]
                {
                    new PeerInfo { node_id = "tls:abc123", address = "10.0.0.1:35000" },
                    new PeerInfo { node_id = "tls:def456", address = "10.0.0.2:35000" },
                    new PeerInfo { node_id = "tls:ghi789", address = "10.0.0.3:35000" }
                }
            });

            // Act
            var peers = await _networkInfoService.GetPeersAsync();

            // Assert
            Assert.IsNotNull(peers);
            Assert.AreEqual(3, peers.Length);
            Assert.AreEqual("tls:abc123", peers[0].NodeId);
            Assert.AreEqual("10.0.0.1:35000", peers[0].Address);
            Assert.IsTrue(_mockClient.WasCalled("info_get_peers"));
        }

        [Test]
        public async Task GetPeersAsync_NoPeers_ReturnsNull()
        {
            // Arrange - no mock response

            // Act
            var peers = await _networkInfoService.GetPeersAsync();

            // Assert
            Assert.IsNull(peers);
        }

        #endregion

        #region GetChainspecAsync Tests

        [Test]
        public async Task GetChainspecAsync_ReturnsChainspec()
        {
            // Arrange
            _mockClient.SetupResponse("info_get_chainspec", new ChainspecResponse
            {
                api_version = "2.0.0",
                chainspec_bytes = new ChainspecBytesData
                {
                    chainspec_bytes = "base64encodeddata..."
                }
            });

            // Act
            var chainspec = await _networkInfoService.GetChainspecAsync();

            // Assert
            Assert.IsNotNull(chainspec);
            Assert.AreEqual("2.0.0", chainspec.ApiVersion);
            Assert.IsNotNull(chainspec.ChainspecBytes);
            Assert.IsTrue(_mockClient.WasCalled("info_get_chainspec"));
        }

        #endregion
    }
}
