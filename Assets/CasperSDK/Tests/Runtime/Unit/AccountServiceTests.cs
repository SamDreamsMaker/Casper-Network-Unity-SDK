using NUnit.Framework;
using CasperSDK.Core.Configuration;
using CasperSDK.Services.Account;
using CasperSDK.Tests.Mocks;
using System;
using System.Threading.Tasks;

namespace CasperSDK.Tests.Unit
{
    /// <summary>
    /// Unit tests for AccountService
    /// Tests balance queries, account info, and key generation
    /// </summary>
    [TestFixture]
    public class AccountServiceTests
    {
        private MockNetworkClient _mockClient;
        private NetworkConfig _config;
        private AccountService _accountService;

        [SetUp]
        public void Setup()
        {
            _mockClient = new MockNetworkClient();
            _config = UnityEngine.ScriptableObject.CreateInstance<NetworkConfig>();
            _accountService = new AccountService(_mockClient, _config);
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

        #region GetBalanceAsync Tests

        [Test]
        public async Task GetBalanceAsync_ValidPublicKey_ReturnsBalance()
        {
            // Arrange
            const string publicKey = "0203a8eb50fc1d6e50cc02f96e7de5c0a29f7de2ec7093f4d73aab3dd2a35aff88a5";
            const string expectedBalance = "1000000000";

            // Setup mock responses for the 3-step balance query
            _mockClient.SetupResponse("info_get_status", new TestStatusInfoResponse
            {
                last_added_block_info = new TestBlockInfo { hash = "abc123" }
            });
            
            _mockClient.SetupResponse("state_get_account_info", new TestAccountInfoResponse
            {
                account = new TestAccountData { main_purse = "uref-abcd1234-007" }
            });
            
            _mockClient.SetupResponse("state_get_balance", new TestBalanceResponse
            {
                balance_value = expectedBalance
            });

            // Act
            var balance = await _accountService.GetBalanceAsync(publicKey);

            // Assert
            Assert.IsNotNull(balance);
            Assert.AreEqual(expectedBalance, balance);
            
            // Verify correct RPC calls were made
            Assert.IsTrue(_mockClient.WasCalled("info_get_status"));
            Assert.IsTrue(_mockClient.WasCalled("state_get_account_info"));
            Assert.IsTrue(_mockClient.WasCalled("state_get_balance"));
        }

        [Test]
        public void GetBalanceAsync_NullPublicKey_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _accountService.GetBalanceAsync(null);
            });
        }

        [Test]
        public void GetBalanceAsync_EmptyPublicKey_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _accountService.GetBalanceAsync("");
            });
        }

        [Test]
        public void GetBalanceAsync_NetworkError_ThrowsException()
        {
            // Arrange
            const string publicKey = "01validkey123";
            _mockClient.ThrowOnNextCall = new Exception("Network timeout");

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () =>
            {
                await _accountService.GetBalanceAsync(publicKey);
            });
        }

        #endregion

        #region GetAccountAsync Tests

        [Test]
        public async Task GetAccountAsync_ValidPublicKey_ReturnsAccountInfo()
        {
            // Arrange
            const string publicKey = "01abc123def456";

            _mockClient.SetupResponse("state_get_account_info", new TestAccountInfoResponse
            {
                account = new TestAccountData
                {
                    account_hash = "account-hash-abc123",
                    main_purse = "uref-purse123-007",
                    named_keys = new object[] { }
                }
            });

            // Act
            var account = await _accountService.GetAccountAsync(publicKey);

            // Assert
            Assert.IsNotNull(account);
            Assert.IsTrue(_mockClient.WasCalled("state_get_account_info"));
        }

        [Test]
        public void GetAccountAsync_NullPublicKey_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _accountService.GetAccountAsync(null);
            });
        }

        #endregion

        #region GenerateKeyPairAsync Tests

        [Test]
        public async Task GenerateKeyPairAsync_ED25519_ReturnsValidKeyPair()
        {
            // Act
            var keyPair = await _accountService.GenerateKeyPairAsync(CasperSDK.Models.KeyAlgorithm.ED25519);

            // Assert
            Assert.IsNotNull(keyPair);
            Assert.IsNotNull(keyPair.PublicKeyHex);
            Assert.IsNotNull(keyPair.AccountHash);
            Assert.AreEqual(CasperSDK.Models.KeyAlgorithm.ED25519, keyPair.Algorithm);
            Assert.IsTrue(keyPair.PublicKeyHex.StartsWith("01")); // ED25519 prefix
        }

        [Test]
        public async Task GenerateKeyPairAsync_SECP256K1_ReturnsValidKeyPair()
        {
            // Act
            var keyPair = await _accountService.GenerateKeyPairAsync(CasperSDK.Models.KeyAlgorithm.SECP256K1);

            // Assert
            Assert.IsNotNull(keyPair);
            Assert.IsNotNull(keyPair.PublicKeyHex);
            Assert.AreEqual(CasperSDK.Models.KeyAlgorithm.SECP256K1, keyPair.Algorithm);
            Assert.IsTrue(keyPair.PublicKeyHex.StartsWith("02")); // SECP256K1 prefix
        }

        #endregion

        #region ImportAccountAsync Tests

        [Test]
        public async Task ImportAccountAsync_ValidSecretKey_ReturnsAccount()
        {
            // Arrange - 64 hex chars (32 bytes) for a valid secret key
            const string secretKeyHex = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

            // Act
            var account = await _accountService.ImportAccountAsync(secretKeyHex, CasperSDK.Models.KeyAlgorithm.ED25519);

            // Assert
            Assert.IsNotNull(account);
            Assert.IsNotNull(account.PublicKeyHex);
        }

        [Test]
        public void ImportAccountAsync_NullSecretKey_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _accountService.ImportAccountAsync(null, CasperSDK.Models.KeyAlgorithm.ED25519);
            });
        }

        #endregion
    }
}
