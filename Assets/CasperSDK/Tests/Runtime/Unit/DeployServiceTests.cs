using NUnit.Framework;
using CasperSDK.Core.Configuration;
using CasperSDK.Services.Deploy;
using CasperSDK.Models.RPC;
using CasperSDK.Tests.Mocks;
using System;
using System.Threading.Tasks;

namespace CasperSDK.Tests.Unit
{
    /// <summary>
    /// Unit tests for DeployService
    /// Tests deploy queries, status checking, and submission
    /// </summary>
    [TestFixture]
    public class DeployServiceTests
    {
        private MockNetworkClient _mockClient;
        private NetworkConfig _config;
        private DeployService _deployService;

        [SetUp]
        public void Setup()
        {
            _mockClient = new MockNetworkClient();
            _config = UnityEngine.ScriptableObject.CreateInstance<NetworkConfig>();
            _deployService = new DeployService(_mockClient, _config);
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

        #region GetDeployAsync Tests

        [Test]
        public async Task GetDeployAsync_ValidHash_ReturnsDeploy()
        {
            // Arrange
            const string deployHash = "deploy-hash-abc123";
            
            _mockClient.SetupResponse("info_get_deploy", new TestDeployResponse
            {
                api_version = "2.0.0",
                deploy = new TestDeployData
                {
                    hash = deployHash,
                    header = new TestDeployHeader
                    {
                        account = "01sender123",
                        timestamp = "2024-01-01T00:00:00Z",
                        chain_name = "casper-test"
                    }
                },
                execution_results = new TestExecutionResultWrapper[0]
            });

            // Act
            var deploy = await _deployService.GetDeployAsync(deployHash);

            // Assert
            Assert.IsNotNull(deploy);
            Assert.AreEqual(deployHash, deploy.Hash);
            Assert.IsTrue(_mockClient.WasCalled("info_get_deploy"));
        }

        [Test]
        public void GetDeployAsync_NullHash_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _deployService.GetDeployAsync(null);
            });
        }

        [Test]
        public void GetDeployAsync_EmptyHash_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _deployService.GetDeployAsync("");
            });
        }

        #endregion

        #region GetDeployStatusAsync Tests

        [Test]
        public async Task GetDeployStatusAsync_PendingDeploy_ReturnsPending()
        {
            // Arrange
            const string deployHash = "pending-deploy-hash";
            
            _mockClient.SetupResponse("info_get_deploy", new TestDeployResponse
            {
                deploy = new TestDeployData { hash = deployHash },
                execution_results = new TestExecutionResultWrapper[0] // Empty = pending
            });

            // Act
            var status = await _deployService.GetDeployStatusAsync(deployHash);

            // Assert
            Assert.IsNotNull(status);
            Assert.AreEqual(DeployStatus.Pending, status.Status);
        }

        [Test]
        public async Task GetDeployStatusAsync_SuccessfulDeploy_ReturnsSuccess()
        {
            // Arrange
            const string deployHash = "success-deploy-hash";
            
            _mockClient.SetupResponse("info_get_deploy", new TestDeployResponse
            {
                deploy = new TestDeployData { hash = deployHash },
                execution_results = new[]
                {
                    new TestExecutionResultWrapper
                    {
                        block_hash = "block-abc",
                        result = new TestExecutionResult
                        {
                            Success = new TestSuccessResult { cost = "1000000" }
                        }
                    }
                }
            });

            // Act
            var status = await _deployService.GetDeployStatusAsync(deployHash);

            // Assert
            Assert.IsNotNull(status);
            Assert.AreEqual(DeployStatus.Success, status.Status);
            Assert.AreEqual("block-abc", status.BlockHash);
            Assert.AreEqual("1000000", status.Cost);
        }

        [Test]
        public async Task GetDeployStatusAsync_FailedDeploy_ReturnsFailed()
        {
            // Arrange
            const string deployHash = "failed-deploy-hash";
            
            _mockClient.SetupResponse("info_get_deploy", new TestDeployResponse
            {
                deploy = new TestDeployData { hash = deployHash },
                execution_results = new[]
                {
                    new TestExecutionResultWrapper
                    {
                        block_hash = "block-xyz",
                        result = new TestExecutionResult
                        {
                            Failure = new TestFailureResult
                            {
                                error_message = "Out of gas",
                                cost = "500000"
                            }
                        }
                    }
                }
            });

            // Act
            var status = await _deployService.GetDeployStatusAsync(deployHash);

            // Assert
            Assert.IsNotNull(status);
            Assert.AreEqual(DeployStatus.Failed, status.Status);
            Assert.AreEqual("Out of gas", status.ErrorMessage);
        }

        [Test]
        public async Task GetDeployStatusAsync_DeployNotFound_ReturnsNotFound()
        {
            // Arrange - no mock response = deploy not found

            // Act
            var status = await _deployService.GetDeployStatusAsync("nonexistent-hash");

            // Assert
            Assert.IsNotNull(status);
            Assert.AreEqual(DeployStatus.NotFound, status.Status);
        }

        #endregion

        #region SubmitDeployAsync Tests

        [Test]
        public async Task SubmitDeployAsync_ValidDeploy_ReturnsHash()
        {
            // Arrange
            const string expectedHash = "submitted-deploy-hash";
            var mockDeploy = new { header = "test", session = "test" };
            
            _mockClient.SetupResponse("account_put_deploy", new TestDeploySubmitResponse
            {
                api_version = "2.0.0",
                deploy_hash = expectedHash
            });

            // Act
            var hash = await _deployService.SubmitDeployAsync(mockDeploy);

            // Assert
            Assert.AreEqual(expectedHash, hash);
            Assert.IsTrue(_mockClient.WasCalled("account_put_deploy"));
        }

        [Test]
        public void SubmitDeployAsync_NullDeploy_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _deployService.SubmitDeployAsync(null);
            });
        }

        #endregion
    }
}
