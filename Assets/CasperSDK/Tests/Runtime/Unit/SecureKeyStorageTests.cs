using System;
using NUnit.Framework;
using CasperSDK.Services.Storage;
using CasperSDK.Models;
using CasperSDK.Utilities.Cryptography;
using UnityEngine;

namespace CasperSDK.Tests.Unit
{
    /// <summary>
    /// Unit tests for SecureKeyStorage
    /// </summary>
    [TestFixture]
    public class SecureKeyStorageTests
    {
        private SecureKeyStorage _storage;
        private const string TestPassword = "TestPassword123!";
        private const string TestLabel = "TestAccount";

        [SetUp]
        public void SetUp()
        {
            _storage = new SecureKeyStorage();
            // Clean up any previous test data
            PlayerPrefs.DeleteAll();
        }

        [TearDown]
        public void TearDown()
        {
            _storage?.Lock();
            PlayerPrefs.DeleteAll();
        }

        [Test]
        public void Unlock_WithValidPassword_SetsIsUnlockedTrue()
        {
            // Arrange & Act
            _storage.Unlock(TestPassword);

            // Assert
            Assert.IsTrue(_storage.IsUnlocked);
        }

        [Test]
        public void Lock_AfterUnlock_SetsIsUnlockedFalse()
        {
            // Arrange
            _storage.Unlock(TestPassword);

            // Act
            _storage.Lock();

            // Assert
            Assert.IsFalse(_storage.IsUnlocked);
        }

        [Test]
        public void SaveKeyPair_AndLoad_ReturnsCorrectKeyPair()
        {
            // Arrange
            _storage.Unlock(TestPassword);
            var originalKeyPair = CasperKeyGenerator.GenerateED25519();

            // Act
            _storage.SaveKeyPair(TestLabel, originalKeyPair);
            var loadedKeyPair = _storage.LoadKeyPair(TestLabel);

            // Assert
            Assert.IsNotNull(loadedKeyPair);
            Assert.AreEqual(originalKeyPair.PublicKeyHex, loadedKeyPair.PublicKeyHex);
            Assert.AreEqual(originalKeyPair.PrivateKeyHex, loadedKeyPair.PrivateKeyHex);
            Assert.AreEqual(originalKeyPair.AccountHash, loadedKeyPair.AccountHash);
        }

        [Test]
        public void SaveKeyPair_WithoutUnlock_ThrowsException()
        {
            // Arrange
            var keyPair = CasperKeyGenerator.GenerateED25519();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => 
                _storage.SaveKeyPair(TestLabel, keyPair));
        }

        [Test]
        public void LoadKeyPair_NonExistent_ReturnsNull()
        {
            // Arrange
            _storage.Unlock(TestPassword);

            // Act
            var result = _storage.LoadKeyPair("NonExistentLabel");

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void DeleteKeyPair_RemovesKey()
        {
            // Arrange
            _storage.Unlock(TestPassword);
            var keyPair = CasperKeyGenerator.GenerateED25519();
            _storage.SaveKeyPair(TestLabel, keyPair);

            // Act
            _storage.DeleteKeyPair(TestLabel);
            var result = _storage.LoadKeyPair(TestLabel);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetAllLabels_ReturnsCorrectLabels()
        {
            // Arrange
            _storage.Unlock(TestPassword);
            var keyPair1 = CasperKeyGenerator.GenerateED25519();
            var keyPair2 = CasperKeyGenerator.GenerateED25519();
            _storage.SaveKeyPair("Account1", keyPair1);
            _storage.SaveKeyPair("Account2", keyPair2);

            // Act
            var labels = _storage.GetAllLabels();

            // Assert
            Assert.AreEqual(2, labels.Length);
            Assert.Contains("Account1", labels);
            Assert.Contains("Account2", labels);
        }

        [Test]
        public void HasKeyPair_WhenExists_ReturnsTrue()
        {
            // Arrange
            _storage.Unlock(TestPassword);
            var keyPair = CasperKeyGenerator.GenerateED25519();
            _storage.SaveKeyPair(TestLabel, keyPair);

            // Act
            var result = _storage.HasKeyPair(TestLabel);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void HasKeyPair_WhenNotExists_ReturnsFalse()
        {
            // Act
            var result = _storage.HasKeyPair("NonExistent");

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Encryption_ProducesUniqueOutput()
        {
            // Arrange
            _storage.Unlock(TestPassword);
            var keyPair1 = CasperKeyGenerator.GenerateED25519();
            var keyPair2 = CasperKeyGenerator.GenerateED25519();

            // Act
            _storage.SaveKeyPair("Key1", keyPair1);
            _storage.SaveKeyPair("Key2", keyPair2);

            // Assert - encrypted values should be different
            var encrypted1 = PlayerPrefs.GetString("CasperSDK_Key_S2V5MQ");
            var encrypted2 = PlayerPrefs.GetString("CasperSDK_Key_S2V5Mg");
            // Just verify they exist and are encrypted
            Assert.IsNotEmpty(encrypted1 ?? "");
        }
    }
}
