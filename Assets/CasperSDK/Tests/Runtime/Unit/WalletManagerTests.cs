using System;
using NUnit.Framework;
using CasperSDK.Services.Wallet;
using CasperSDK.Models;
using CasperSDK.Utilities.Cryptography;
using UnityEngine;

namespace CasperSDK.Tests.Unit
{
    /// <summary>
    /// Unit tests for WalletManager
    /// </summary>
    [TestFixture]
    public class WalletManagerTests
    {
        private WalletManager _walletManager;
        private const string TestPassword = "TestWalletPassword123!";

        [SetUp]
        public void SetUp()
        {
            _walletManager = new WalletManager(enableLogging: false);
            PlayerPrefs.DeleteAll();
        }

        [TearDown]
        public void TearDown()
        {
            _walletManager?.Lock();
            PlayerPrefs.DeleteAll();
        }

        [Test]
        public void Unlock_SetsIsUnlockedTrue()
        {
            // Act
            _walletManager.Unlock(TestPassword);

            // Assert
            Assert.IsTrue(_walletManager.IsUnlocked);
        }

        [Test]
        public void Lock_SetsIsUnlockedFalse()
        {
            // Arrange
            _walletManager.Unlock(TestPassword);

            // Act
            _walletManager.Lock();

            // Assert
            Assert.IsFalse(_walletManager.IsUnlocked);
        }

        [Test]
        public void CreateAccount_AddsAccountToList()
        {
            // Arrange
            _walletManager.Unlock(TestPassword);

            // Act
            var account = _walletManager.CreateAccount("MyAccount");

            // Assert
            Assert.AreEqual(1, _walletManager.AccountCount);
            Assert.AreEqual("MyAccount", account.Label);
            Assert.IsNotNull(account.PublicKey);
            Assert.IsNotNull(account.KeyPair);
        }

        [Test]
        public void CreateAccount_ED25519_HasCorrectPrefix()
        {
            // Arrange
            _walletManager.Unlock(TestPassword);

            // Act
            var account = _walletManager.CreateAccount("ED25519Account", KeyAlgorithm.ED25519);

            // Assert
            Assert.IsTrue(account.PublicKey.StartsWith("01"));
        }

        [Test]
        public void CreateAccount_SECP256K1_HasCorrectPrefix()
        {
            // Arrange
            _walletManager.Unlock(TestPassword);

            // Act
            var account = _walletManager.CreateAccount("SECP256K1Account", KeyAlgorithm.SECP256K1);

            // Assert
            Assert.IsTrue(account.PublicKey.StartsWith("02"));
        }

        [Test]
        public void CreateAccount_DuplicateLabel_ThrowsException()
        {
            // Arrange
            _walletManager.Unlock(TestPassword);
            _walletManager.CreateAccount("DuplicateLabel");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _walletManager.CreateAccount("DuplicateLabel"));
        }

        [Test]
        public void DeleteAccount_RemovesFromList()
        {
            // Arrange
            _walletManager.Unlock(TestPassword);
            _walletManager.CreateAccount("ToDelete");

            // Act
            _walletManager.DeleteAccount("ToDelete");

            // Assert
            Assert.AreEqual(0, _walletManager.AccountCount);
        }

        [Test]
        public void SetActiveAccount_UpdatesActiveAccount()
        {
            // Arrange
            _walletManager.Unlock(TestPassword);
            _walletManager.CreateAccount("Account1");
            _walletManager.CreateAccount("Account2");

            // Act
            _walletManager.SetActiveAccount("Account2");

            // Assert
            Assert.AreEqual("Account2", _walletManager.ActiveAccount.Label);
        }

        [Test]
        public void RenameAccount_UpdatesLabel()
        {
            // Arrange
            _walletManager.Unlock(TestPassword);
            _walletManager.CreateAccount("OldName");

            // Act
            _walletManager.RenameAccount("OldName", "NewName");

            // Assert
            Assert.IsNull(_walletManager.GetAccount("OldName"));
            Assert.IsNotNull(_walletManager.GetAccount("NewName"));
        }

        [Test]
        public void ImportAccount_AddsExistingKeyPair()
        {
            // Arrange
            _walletManager.Unlock(TestPassword);
            var keyPair = CasperKeyGenerator.GenerateED25519();

            // Act
            var account = _walletManager.ImportAccount("ImportedAccount", keyPair);

            // Assert
            Assert.AreEqual(keyPair.PublicKeyHex, account.PublicKey);
            Assert.AreEqual(1, _walletManager.AccountCount);
        }

        [Test]
        public void GetAccount_NonExistent_ReturnsNull()
        {
            // Arrange
            _walletManager.Unlock(TestPassword);

            // Act
            var result = _walletManager.GetAccount("NonExistent");

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void AccountExists_WhenExists_ReturnsTrue()
        {
            // Arrange
            _walletManager.Unlock(TestPassword);
            _walletManager.CreateAccount("ExistingAccount");

            // Act
            var result = _walletManager.AccountExists("ExistingAccount");

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void OnActiveAccountChanged_EventFired()
        {
            // Arrange
            _walletManager.Unlock(TestPassword);
            WalletAccount receivedAccount = null;
            _walletManager.OnActiveAccountChanged += (account) => receivedAccount = account;
            _walletManager.CreateAccount("Account1");
            _walletManager.CreateAccount("Account2");

            // Act
            _walletManager.SetActiveAccount("Account2");

            // Assert
            Assert.IsNotNull(receivedAccount);
            Assert.AreEqual("Account2", receivedAccount.Label);
        }

        [Test]
        public void ShortPublicKey_FormatsCorrectly()
        {
            // Arrange
            _walletManager.Unlock(TestPassword);
            var account = _walletManager.CreateAccount("ShortKeyTest");

            // Act
            var shortKey = account.ShortPublicKey;

            // Assert
            Assert.IsTrue(shortKey.Contains("..."));
            Assert.IsTrue(shortKey.Length < account.PublicKey.Length);
        }
    }
}
