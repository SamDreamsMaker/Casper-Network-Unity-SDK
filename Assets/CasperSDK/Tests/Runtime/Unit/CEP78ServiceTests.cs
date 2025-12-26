using System;
using System.Threading.Tasks;
using NUnit.Framework;
using CasperSDK.Services.NFT;
using CasperSDK.Models;
using CasperSDK.Utilities.Cryptography;

namespace CasperSDK.Tests.Unit
{
    /// <summary>
    /// Unit tests for CEP78Service - testing models and basic functionality
    /// </summary>
    [TestFixture]
    public class CEP78ServiceTests
    {
        private KeyPair _testKeyPair;

        [SetUp]
        public void SetUp()
        {
            _testKeyPair = CasperKeyGenerator.GenerateED25519();
        }

        [Test]
        public void NFTMetadata_Properties_AreSettable()
        {
            // Arrange & Act
            var metadata = new NFTMetadata
            {
                Name = "Test NFT",
                Description = "A test NFT",
                Image = "https://example.com/image.png"
            };

            // Assert
            Assert.AreEqual("Test NFT", metadata.Name);
            Assert.AreEqual("A test NFT", metadata.Description);
            Assert.AreEqual("https://example.com/image.png", metadata.Image);
        }

        [Test]
        public void NFTAttribute_Properties_AreSettable()
        {
            // Arrange & Act
            var attr = new NFTAttribute
            {
                TraitType = "Rarity",
                Value = "Legendary"
            };

            // Assert
            Assert.AreEqual("Rarity", attr.TraitType);
            Assert.AreEqual("Legendary", attr.Value);
        }

        [Test]
        public void NFTMintResult_Properties_AreSettable()
        {
            // Arrange & Act
            var result = new NFTMintResult
            {
                Success = true,
                DeployHash = "abc123",
                TokenId = "1",
                TokenName = "MyNFT",
                ErrorMessage = null
            };

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual("abc123", result.DeployHash);
            Assert.AreEqual("1", result.TokenId);
            Assert.AreEqual("MyNFT", result.TokenName);
            Assert.IsNull(result.ErrorMessage);
        }

        [Test]
        public void NFTTransferResult_Properties_AreSettable()
        {
            // Arrange & Act
            var result = new NFTTransferResult
            {
                Success = true,
                DeployHash = "def456",
                TokenId = 5UL,
                NewOwner = "01abc...",
                ErrorMessage = null
            };

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual("def456", result.DeployHash);
            Assert.AreEqual(5UL, result.TokenId);
            Assert.AreEqual("01abc...", result.NewOwner);
        }

        [Test]
        public void NFTBurnResult_Properties_AreSettable()
        {
            // Arrange & Act
            var result = new NFTBurnResult
            {
                Success = false,
                DeployHash = null,
                TokenId = 10UL,
                ErrorMessage = "Burn failed"
            };

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.DeployHash);
            Assert.AreEqual(10UL, result.TokenId);
            Assert.AreEqual("Burn failed", result.ErrorMessage);
        }

        [Test]
        public void NFTMetadata_WithAttributes_CanBeCreated()
        {
            // Arrange & Act
            var metadata = new NFTMetadata
            {
                Name = "Cool NFT",
                Description = "Very cool",
                Image = "https://img.url",
                Attributes = new NFTAttribute[]
                {
                    new NFTAttribute { TraitType = "Rarity", Value = "Legendary" },
                    new NFTAttribute { TraitType = "Power", Value = "100" }
                }
            };

            // Assert
            Assert.IsNotNull(metadata.Attributes);
            Assert.AreEqual(2, metadata.Attributes.Length);
            Assert.AreEqual("Rarity", metadata.Attributes[0].TraitType);
            Assert.AreEqual("Power", metadata.Attributes[1].TraitType);
        }

        [Test]
        public void KeyPair_FromGenerator_HasValidFormat()
        {
            // Assert
            Assert.IsNotNull(_testKeyPair);
            Assert.IsNotNull(_testKeyPair.PublicKeyHex);
            Assert.IsNotNull(_testKeyPair.PrivateKeyHex);
            Assert.IsTrue(_testKeyPair.PublicKeyHex.StartsWith("01")); // ED25519 prefix
        }

        [Test]
        public void KeyPair_SECP256K1_HasValidPrefix()
        {
            // Arrange & Act
            var keyPair = CasperKeyGenerator.GenerateSECP256K1();

            // Assert
            Assert.IsTrue(keyPair.PublicKeyHex.StartsWith("02"));
        }
    }
}
