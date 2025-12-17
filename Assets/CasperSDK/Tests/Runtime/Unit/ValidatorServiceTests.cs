using NUnit.Framework;
using CasperSDK.Core.Configuration;
using CasperSDK.Services.Validator;
using CasperSDK.Tests.Mocks;
using System;
using System.Threading.Tasks;

namespace CasperSDK.Tests.Unit
{
    /// <summary>
    /// Unit tests for ValidatorService
    /// Tests auction info, validators list, and validator lookup
    /// </summary>
    [TestFixture]
    public class ValidatorServiceTests
    {
        private MockNetworkClient _mockClient;
        private NetworkConfig _config;
        private ValidatorService _validatorService;

        [SetUp]
        public void Setup()
        {
            _mockClient = new MockNetworkClient();
            _config = UnityEngine.ScriptableObject.CreateInstance<NetworkConfig>();
            _config.EnableLogging = false;
            _validatorService = new ValidatorService(_mockClient, _config);
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

        #region GetAuctionInfoAsync Tests

        [Test]
        public async Task GetAuctionInfoAsync_ReturnsAuctionInfo()
        {
            // Arrange
            _mockClient.SetupResponse("state_get_auction_info", new TestAuctionInfoResponse
            {
                api_version = "2.0.0",
                auction_state = new TestAuctionState
                {
                    state_root_hash = "state-root-xyz",
                    block_height = 12345,
                    era_validators = new[]
                    {
                        new TestEraValidator
                        {
                            era_id = 100,
                            validator_weights = new[]
                            {
                                new TestValidatorWeight { public_key = "01validator1", weight = "1000000000" }
                            }
                        }
                    },
                    bids = new[]
                    {
                        new TestBidInfo
                        {
                            public_key = "01validator1",
                            bid = new TestBidData
                            {
                                bonding_purse = "uref-purse123",
                                staked_amount = "1000000000",
                                delegation_rate = 10,
                                inactive = false
                            }
                        }
                    }
                }
            });

            // Act
            var auctionInfo = await _validatorService.GetAuctionInfoAsync();

            // Assert
            Assert.IsNotNull(auctionInfo);
            Assert.AreEqual(12345, auctionInfo.BlockHeight);
            Assert.AreEqual("state-root-xyz", auctionInfo.StateRootHash);
            Assert.IsNotNull(auctionInfo.EraValidators);
            Assert.IsNotNull(auctionInfo.Bids);
            Assert.IsTrue(_mockClient.WasCalled("state_get_auction_info"));
        }

        [Test]
        public async Task GetAuctionInfoAsync_NoData_ReturnsNull()
        {
            // Arrange - no mock response

            // Act
            var auctionInfo = await _validatorService.GetAuctionInfoAsync();

            // Assert
            Assert.IsNull(auctionInfo);
        }

        #endregion

        #region GetValidatorsAsync Tests

        [Test]
        public async Task GetValidatorsAsync_ReturnsValidatorList()
        {
            // Arrange
            _mockClient.SetupResponse("state_get_auction_info", new TestAuctionInfoResponse
            {
                auction_state = new TestAuctionState
                {
                    era_validators = new[]
                    {
                        new TestEraValidator
                        {
                            era_id = 100,
                            validator_weights = new[]
                            {
                                new TestValidatorWeight { public_key = "01validator1", weight = "1000000000" },
                                new TestValidatorWeight { public_key = "01validator2", weight = "2000000000" },
                                new TestValidatorWeight { public_key = "01validator3", weight = "3000000000" }
                            }
                        }
                    }
                }
            });

            // Act
            var validators = await _validatorService.GetValidatorsAsync();

            // Assert
            Assert.IsNotNull(validators);
            Assert.AreEqual(3, validators.Length);
            Assert.AreEqual("01validator1", validators[0].PublicKey);
            Assert.AreEqual(100, validators[0].EraId);
        }

        [Test]
        public async Task GetValidatorsAsync_EmptyEra_ReturnsEmptyArray()
        {
            // Arrange
            _mockClient.SetupResponse("state_get_auction_info", new TestAuctionInfoResponse
            {
                auction_state = new TestAuctionState
                {
                    era_validators = new TestEraValidator[0]
                }
            });

            // Act
            var validators = await _validatorService.GetValidatorsAsync();

            // Assert
            Assert.IsNotNull(validators);
            Assert.AreEqual(0, validators.Length);
        }

        #endregion

        #region GetValidatorByKeyAsync Tests

        [Test]
        public async Task GetValidatorByKeyAsync_ValidKey_ReturnsValidator()
        {
            // Arrange
            const string publicKey = "01validator1";
            
            _mockClient.SetupResponse("state_get_auction_info", new TestAuctionInfoResponse
            {
                auction_state = new TestAuctionState
                {
                    bids = new[]
                    {
                        new TestBidInfo
                        {
                            public_key = publicKey,
                            bid = new TestBidData
                            {
                                bonding_purse = "uref-purse123",
                                staked_amount = "5000000000",
                                delegation_rate = 15,
                                inactive = false
                            }
                        }
                    }
                }
            });

            // Act
            var validator = await _validatorService.GetValidatorByKeyAsync(publicKey);

            // Assert
            Assert.IsNotNull(validator);
            Assert.AreEqual(publicKey, validator.PublicKey);
            Assert.AreEqual("5000000000", validator.StakedAmount);
            Assert.AreEqual(15, validator.DelegationRate);
            Assert.IsFalse(validator.Inactive);
        }

        [Test]
        public async Task GetValidatorByKeyAsync_InvalidKey_ReturnsNull()
        {
            // Arrange
            _mockClient.SetupResponse("state_get_auction_info", new TestAuctionInfoResponse
            {
                auction_state = new TestAuctionState
                {
                    bids = new[]
                    {
                        new TestBidInfo { public_key = "01othervalidator" }
                    }
                }
            });

            // Act
            var validator = await _validatorService.GetValidatorByKeyAsync("01nonexistent");

            // Assert
            Assert.IsNull(validator);
        }

        [Test]
        public void GetValidatorByKeyAsync_NullKey_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _validatorService.GetValidatorByKeyAsync(null);
            });
        }

        #endregion
    }
}
