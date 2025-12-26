using NUnit.Framework;
using CasperSDK.Utilities.Cryptography;
using CasperSDK.Models;

namespace CasperSDK.Tests.Unit
{
    /// <summary>
    /// Unit tests for CryptoHelper and KeyPairGenerator
    /// </summary>
    [TestFixture]
    public class CryptoHelperTests
    {
        #region BytesToHex Tests

        [Test]
        public void BytesToHex_ValidBytes_ReturnsCorrectHex()
        {
            // Arrange
            byte[] bytes = { 0xDE, 0xAD, 0xBE, 0xEF };

            // Act
            var hex = CryptoHelper.BytesToHex(bytes);

            // Assert
            Assert.AreEqual("deadbeef", hex);
        }

        [Test]
        public void BytesToHex_EmptyArray_ReturnsEmptyString()
        {
            // Act
            var hex = CryptoHelper.BytesToHex(new byte[0]);

            // Assert
            Assert.AreEqual("", hex);
        }

        [Test]
        public void BytesToHex_NullArray_ReturnsEmptyString()
        {
            // Act
            var hex = CryptoHelper.BytesToHex(null);

            // Assert
            Assert.AreEqual("", hex);
        }

        #endregion

        #region HexToBytes Tests

        [Test]
        public void HexToBytes_ValidHex_ReturnsCorrectBytes()
        {
            // Arrange
            var hex = "deadbeef";

            // Act
            var bytes = CryptoHelper.HexToBytes(hex);

            // Assert
            Assert.AreEqual(4, bytes.Length);
            Assert.AreEqual(0xDE, bytes[0]);
            Assert.AreEqual(0xAD, bytes[1]);
            Assert.AreEqual(0xBE, bytes[2]);
            Assert.AreEqual(0xEF, bytes[3]);
        }

        [Test]
        public void HexToBytes_UpperCaseHex_ReturnsCorrectBytes()
        {
            // Arrange
            var hex = "DEADBEEF";

            // Act
            var bytes = CryptoHelper.HexToBytes(hex);

            // Assert
            Assert.AreEqual(4, bytes.Length);
            Assert.AreEqual(0xDE, bytes[0]);
        }

        [Test]
        public void HexToBytes_With0xPrefix_ReturnsCorrectBytes()
        {
            // Arrange
            var hex = "0xDEAD";

            // Act
            var bytes = CryptoHelper.HexToBytes(hex);

            // Assert
            Assert.AreEqual(2, bytes.Length);
            Assert.AreEqual(0xDE, bytes[0]);
            Assert.AreEqual(0xAD, bytes[1]);
        }

        [Test]
        public void HexToBytes_OddLengthHex_PadsWithZero()
        {
            // Arrange
            var hex = "ABC"; // Odd length

            // Act
            var bytes = CryptoHelper.HexToBytes(hex);

            // Assert
            Assert.AreEqual(2, bytes.Length);
            Assert.AreEqual(0x0A, bytes[0]);
            Assert.AreEqual(0xBC, bytes[1]);
        }

        #endregion

        #region GenerateSecureRandomBytes Tests

        [Test]
        public void GenerateSecureRandomBytes_ReturnsCorrectLength()
        {
            // Act
            var bytes32 = CryptoHelper.GenerateSecureRandomBytes(32);
            var bytes64 = CryptoHelper.GenerateSecureRandomBytes(64);

            // Assert
            Assert.AreEqual(32, bytes32.Length);
            Assert.AreEqual(64, bytes64.Length);
        }

        [Test]
        public void GenerateSecureRandomBytes_GeneratesDifferentValues()
        {
            // Act
            var bytes1 = CryptoHelper.GenerateSecureRandomBytes(32);
            var bytes2 = CryptoHelper.GenerateSecureRandomBytes(32);

            // Assert - they should be different
            Assert.AreNotEqual(CryptoHelper.BytesToHex(bytes1), CryptoHelper.BytesToHex(bytes2));
        }

        #endregion

        #region ValidatePublicKey Tests

        [Test]
        public void ValidatePublicKey_ValidED25519Key_ReturnsTrue()
        {
            // Arrange - 01 prefix + 64 hex chars (32 bytes)
            var publicKey = "01" + new string('a', 64);

            // Act
            var result = CryptoHelper.ValidatePublicKey(publicKey);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ValidatePublicKey_ValidSECP256K1Key_ReturnsTrue()
        {
            // Arrange - 02 prefix + 66 hex chars (33 bytes)
            var publicKey = "02" + new string('b', 66);

            // Act
            var result = CryptoHelper.ValidatePublicKey(publicKey);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ValidatePublicKey_InvalidPrefix_ReturnsFalse()
        {
            // Arrange - 03 is not a valid prefix
            var publicKey = "03" + new string('a', 64);

            // Act
            var result = CryptoHelper.ValidatePublicKey(publicKey);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ValidatePublicKey_TooShort_ReturnsFalse()
        {
            // Arrange
            var publicKey = "01abc";

            // Act
            var result = CryptoHelper.ValidatePublicKey(publicKey);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ValidatePublicKey_NullOrEmpty_ReturnsFalse()
        {
            Assert.IsFalse(CryptoHelper.ValidatePublicKey(null));
            Assert.IsFalse(CryptoHelper.ValidatePublicKey(""));
        }

        #endregion

        #region ValidateAccountHash Tests

        [Test]
        public void ValidateAccountHash_ValidHash_ReturnsTrue()
        {
            // Arrange - account-hash- prefix + 64 hex chars
            var hash = "account-hash-" + new string('a', 64);

            // Act
            var result = CryptoHelper.ValidateAccountHash(hash);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ValidateAccountHash_InvalidPrefix_ReturnsFalse()
        {
            // Arrange
            var hash = "hash-" + new string('a', 64);

            // Act
            var result = CryptoHelper.ValidateAccountHash(hash);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateAccountHash_WrongLength_ReturnsFalse()
        {
            // Arrange
            var hash = "account-hash-abc";

            // Act
            var result = CryptoHelper.ValidateAccountHash(hash);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region GenerateAccountHash Tests

        [Test]
        public void GenerateAccountHash_ValidED25519Key_ReturnsValidHash()
        {
            // Arrange
            var publicKey = "01" + new string('a', 64);

            // Act
            var hash = CryptoHelper.GenerateAccountHash(publicKey);

            // Assert
            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.StartsWith("account-hash-"));
            Assert.IsTrue(CryptoHelper.ValidateAccountHash(hash));
        }

        [Test]
        public void GenerateAccountHash_SameInputProducesSameOutput()
        {
            // Arrange
            var publicKey = "01abcdef1234567890abcdef1234567890abcdef1234567890abcdef12345678";

            // Act
            var hash1 = CryptoHelper.GenerateAccountHash(publicKey);
            var hash2 = CryptoHelper.GenerateAccountHash(publicKey);

            // Assert
            Assert.AreEqual(hash1, hash2);
        }

        [Test]
        public void GenerateAccountHash_DifferentInputsProduceDifferentOutputs()
        {
            // Arrange
            var publicKey1 = "01" + new string('a', 64);
            var publicKey2 = "01" + new string('b', 64);

            // Act
            var hash1 = CryptoHelper.GenerateAccountHash(publicKey1);
            var hash2 = CryptoHelper.GenerateAccountHash(publicKey2);

            // Assert
            Assert.AreNotEqual(hash1, hash2);
        }

        #endregion
    }

    /// <summary>
    /// Unit tests for KeyPairGenerator
    /// </summary>
    [TestFixture]
    public class KeyPairGeneratorTests
    {
        #region Generate Tests

        [Test]
        public void Generate_ED25519_ReturnsValidKeyPair()
        {
            // Act
            var keyPair = KeyPairGenerator.Generate(KeyAlgorithm.ED25519);

            // Assert
            Assert.IsNotNull(keyPair);
            Assert.IsNotNull(keyPair.PublicKeyHex);
            Assert.IsNotNull(keyPair.PrivateKeyHex);
            Assert.IsNotNull(keyPair.AccountHash);
            Assert.AreEqual(KeyAlgorithm.ED25519, keyPair.Algorithm);
            Assert.IsTrue(keyPair.PublicKeyHex.StartsWith("01"));
        }

        [Test]
        public void Generate_SECP256K1_ReturnsValidKeyPair()
        {
            // Act
            var keyPair = KeyPairGenerator.Generate(KeyAlgorithm.SECP256K1);

            // Assert
            Assert.IsNotNull(keyPair);
            Assert.IsNotNull(keyPair.PublicKeyHex);
            Assert.IsNotNull(keyPair.PrivateKeyHex);
            Assert.IsNotNull(keyPair.AccountHash);
            Assert.AreEqual(KeyAlgorithm.SECP256K1, keyPair.Algorithm);
            Assert.IsTrue(keyPair.PublicKeyHex.StartsWith("02"));
        }

        [Test]
        public void Generate_ED25519_PrivateKeyHasCorrectLength()
        {
            // Act
            var keyPair = KeyPairGenerator.Generate(KeyAlgorithm.ED25519);

            // Assert - 32 bytes = 64 hex chars
            Assert.AreEqual(64, keyPair.PrivateKeyHex.Length);
        }

        [Test]
        public void Generate_GeneratesUniqueKeys()
        {
            // Act
            var keyPair1 = KeyPairGenerator.Generate(KeyAlgorithm.ED25519);
            var keyPair2 = KeyPairGenerator.Generate(KeyAlgorithm.ED25519);

            // Assert
            Assert.AreNotEqual(keyPair1.PublicKeyHex, keyPair2.PublicKeyHex);
            Assert.AreNotEqual(keyPair1.PrivateKeyHex, keyPair2.PrivateKeyHex);
            Assert.AreNotEqual(keyPair1.AccountHash, keyPair2.AccountHash);
        }

        [Test]
        public void Generate_AccountHashIsValid()
        {
            // Act
            var keyPair = KeyPairGenerator.Generate(KeyAlgorithm.ED25519);

            // Assert
            Assert.IsTrue(CryptoHelper.ValidateAccountHash(keyPair.AccountHash));
        }

        #endregion

        #region Import Tests

        [Test]
        public void Import_ValidPrivateKey_ReturnsKeyPair()
        {
            // Arrange - 64 hex chars (32 bytes)
            var privateKeyHex = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

            // Act
            var keyPair = KeyPairGenerator.Import(privateKeyHex, KeyAlgorithm.ED25519);

            // Assert
            Assert.IsNotNull(keyPair);
            Assert.AreEqual(privateKeyHex, keyPair.PrivateKeyHex);
            Assert.IsTrue(keyPair.PublicKeyHex.StartsWith("01"));
        }

        [Test]
        public void Import_SamePrivateKey_ProducesSamePublicKey()
        {
            // Arrange
            var privateKeyHex = "abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789";

            // Act
            var keyPair1 = KeyPairGenerator.Import(privateKeyHex, KeyAlgorithm.ED25519);
            var keyPair2 = KeyPairGenerator.Import(privateKeyHex, KeyAlgorithm.ED25519);

            // Assert
            Assert.AreEqual(keyPair1.PublicKeyHex, keyPair2.PublicKeyHex);
            Assert.AreEqual(keyPair1.AccountHash, keyPair2.AccountHash);
        }

        [Test]
        public void Import_NullPrivateKey_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentException>(() => KeyPairGenerator.Import(null));
        }

        [Test]
        public void Import_EmptyPrivateKey_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentException>(() => KeyPairGenerator.Import(""));
        }

        #endregion

        #region ValidatePrivateKey Tests

        [Test]
        public void ValidatePrivateKey_ValidED25519Key_ReturnsTrue()
        {
            // Arrange - 64 hex chars (32 bytes)
            var privateKey = new string('a', 64);

            // Act
            var result = KeyPairGenerator.ValidatePrivateKey(privateKey, KeyAlgorithm.ED25519);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ValidatePrivateKey_TooShort_ReturnsFalse()
        {
            // Arrange
            var privateKey = "abc123";

            // Act
            var result = KeyPairGenerator.ValidatePrivateKey(privateKey, KeyAlgorithm.ED25519);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ValidatePrivateKey_NullKey_ReturnsFalse()
        {
            // Act
            var result = KeyPairGenerator.ValidatePrivateKey(null, KeyAlgorithm.ED25519);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion
    }
}
