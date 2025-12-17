using NUnit.Framework;
using CasperSDK.Core.Configuration;
using CasperSDK.Services.State;
using CasperSDK.Tests.Mocks;
using System;
using System.Threading.Tasks;

namespace CasperSDK.Tests.Unit
{
    /// <summary>
    /// Unit tests for StateService
    /// Tests global state queries and dictionary operations
    /// </summary>
    [TestFixture]
    public class StateServiceTests
    {
        private MockNetworkClient _mockClient;
        private NetworkConfig _config;
        private StateService _stateService;

        [SetUp]
        public void Setup()
        {
            _mockClient = new MockNetworkClient();
            _config = UnityEngine.ScriptableObject.CreateInstance<NetworkConfig>();
            _config.EnableLogging = false;
            _stateService = new StateService(_mockClient, _config);
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

        #region QueryGlobalStateAsync Tests

        [Test]
        public async Task QueryGlobalStateAsync_ValidKey_ReturnsValue()
        {
            // Arrange
            const string key = "account-hash-abc123";
            
            _mockClient.SetupResponse("query_global_state", new TestQueryGlobalStateResponse
            {
                api_version = "2.0.0",
                stored_value = new { Account = new { main_purse = "uref-xyz" } },
                merkle_proof = "proof-abc123"
            });

            // Act
            var result = await _stateService.QueryGlobalStateAsync(key);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(key, result.Key);
            Assert.IsNotNull(result.StoredValue);
            Assert.IsNotNull(result.MerkleProof);
            Assert.IsTrue(_mockClient.WasCalled("query_global_state"));
        }

        [Test]
        public async Task QueryGlobalStateAsync_WithStateRootHash_UsesHash()
        {
            // Arrange
            const string key = "some-key";
            const string stateRootHash = "state-root-xyz";
            
            _mockClient.SetupResponse("query_global_state", new TestQueryGlobalStateResponse
            {
                stored_value = new { data = "test" }
            });

            // Act
            var result = await _stateService.QueryGlobalStateAsync(key, stateRootHash);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, _mockClient.GetCallCount("query_global_state"));
        }

        [Test]
        public void QueryGlobalStateAsync_NullKey_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _stateService.QueryGlobalStateAsync(null);
            });
        }

        [Test]
        public async Task QueryGlobalStateAsync_KeyNotFound_ReturnsNull()
        {
            // Arrange - no stored_value in response
            _mockClient.SetupResponse("query_global_state", new TestQueryGlobalStateResponse
            {
                api_version = "2.0.0",
                stored_value = null
            });

            // Act
            var result = await _stateService.QueryGlobalStateAsync("nonexistent-key");

            // Assert
            Assert.IsNull(result);
        }

        #endregion

        #region GetDictionaryItemAsync Tests

        [Test]
        public async Task GetDictionaryItemAsync_ValidKey_ReturnsItem()
        {
            // Arrange
            const string dictionaryKey = "my-dict-key";
            const string seedUref = "uref-seed123-007";
            
            _mockClient.SetupResponse("state_get_dictionary_item", new TestDictionaryItemResponse
            {
                api_version = "2.0.0",
                dictionary_key = "dictionary-key-xyz",
                stored_value = new { CLValue = new { bytes = "0500000048656c6c6f" } },
                merkle_proof = "proof-123"
            });

            // Act
            var result = await _stateService.GetDictionaryItemAsync(dictionaryKey, seedUref);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(dictionaryKey, result.Key);
            Assert.IsNotNull(result.StoredValue);
            Assert.IsTrue(_mockClient.WasCalled("state_get_dictionary_item"));
        }

        [Test]
        public void GetDictionaryItemAsync_NullKey_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _stateService.GetDictionaryItemAsync(null, "uref-seed");
            });
        }

        [Test]
        public void GetDictionaryItemAsync_NullSeedUref_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _stateService.GetDictionaryItemAsync("key", null);
            });
        }

        #endregion

        #region GetDictionaryItemByNameAsync Tests

        [Test]
        public async Task GetDictionaryItemByNameAsync_ValidParams_ReturnsItem()
        {
            // Arrange
            const string contractHash = "contract-abc123";
            const string dictionaryName = "my_dictionary";
            const string dictionaryKey = "item_key";
            
            _mockClient.SetupResponse("state_get_dictionary_item", new TestDictionaryItemResponse
            {
                dictionary_key = "generated-dict-key",
                stored_value = new { value = 12345 }
            });

            // Act
            var result = await _stateService.GetDictionaryItemByNameAsync(contractHash, dictionaryName, dictionaryKey);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(dictionaryKey, result.Key);
        }

        [Test]
        public void GetDictionaryItemByNameAsync_NullContractHash_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _stateService.GetDictionaryItemByNameAsync(null, "dict", "key");
            });
        }

        #endregion
    }
}
