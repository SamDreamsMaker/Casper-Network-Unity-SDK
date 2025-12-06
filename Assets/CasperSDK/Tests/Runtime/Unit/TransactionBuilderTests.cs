using NUnit.Framework;
using CasperSDK.Core.Interfaces;
using CasperSDK.Services.Transaction;
using CasperSDK.Models;

namespace CasperSDK.Tests.Unit
{
    /// <summary>
    /// Unit tests for TransactionBuilder
    /// Demonstrates the AAA (Arrange-Act-Assert) pattern
    /// </summary>
    [TestFixture]
    public class TransactionBuilderTests
    {
        private ITransactionBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new TransactionBuilder();
        }

        [Test]
        public void Build_WithValidParameters_ShouldCreateTransaction()
        {
            // Arrange
            const string from = "01abc123sender";
            const string target = "01def456recipient";
            const string amount = "1000000000";

            // Act
            var transaction = _builder
                .SetFrom(from)
                .SetTarget(target)
                .SetAmount(amount)
                .Build();

            // Assert
            Assert.IsNotNull(transaction);
            Assert.AreEqual(from, transaction.From);
            Assert.AreEqual(target, transaction.Target);
            Assert.AreEqual(amount, transaction.Amount);
        }

        [Test]
        public void Build_WithoutFrom_ShouldThrowValidationException()
        {
            // Arrange
            _builder.SetTarget("01target")
                   .SetAmount("1000");

            // Act & Assert
            Assert.Throws<ValidationException>(() => _builder.Build());
        }

        [Test]
        public void Build_WithoutTarget_ShouldThrowValidationException()
        {
            // Arrange
            _builder.SetFrom("01sender")
                   .SetAmount("1000");

            // Act & Assert
            Assert.Throws<ValidationException>(() => _builder.Build());
        }

        [Test]
        public void Build_WithoutAmount_ShouldThrowValidationException()
        {
            // Arrange
            _builder.SetFrom("01sender")
                   .SetTarget("01target");

            // Act & Assert
            Assert.Throws<ValidationException>(() => _builder.Build());
        }

        [Test]
        public void SetAmount_WithInvalidAmount_ShouldThrowValidationException()
        {
            // Act & Assert
            Assert.Throws<ValidationException>(() => _builder.SetAmount("invalid"));
        }

        [Test]
        public void SetAmount_WithNegativeAmount_ShouldThrowValidationException()
        {
            // Act & Assert
            Assert.Throws<ValidationException>(() => _builder.SetAmount("-1000"));
        }

        [Test]
        public void SetGasPrice_WithZero_ShouldThrowValidationException()
        {
            // Act & Assert
            Assert.Throws<ValidationException>(() => _builder.SetGasPrice(0));
        }

        [Test]
        public void SetGasPrice_WithNegative_ShouldThrowValidationException()
        {
            // Act & Assert
            Assert.Throws<ValidationException>(() => _builder.SetGasPrice(-1));
        }

        [Test]
        public void SetTTL_WithZero_ShouldThrowValidationException()
        {
            // Act & Assert
            Assert.Throws<ValidationException>(() => _builder.SetTTL(0));
        }

        [Test]
        public void Build_WithCustomGasPrice_ShouldSetCorrectly()
        {
            // Arrange
            const long customGasPrice = 100;

            // Act
            var transaction = _builder
                .SetFrom("01sender")
                .SetTarget("01target")
                .SetAmount("1000")
                .SetGasPrice(customGasPrice)
                .Build();

            // Assert
            Assert.AreEqual(customGasPrice, transaction.GasPrice);
        }

        [Test]
        public void Build_WithCustomTTL_ShouldSetCorrectly()
        {
            // Arrange
            const long customTTL = 7200000; // 2 hours

            // Act
            var transaction = _builder
                .SetFrom("01sender")
                .SetTarget("01target")
                .SetAmount("1000")
                .SetTTL(customTTL)
                .Build();

            // Assert
            Assert.AreEqual(customTTL, transaction.TTL);
        }

        [Test]
        public void Build_WithTransferId_ShouldSetCorrectly()
        {
            // Arrange
            const ulong transferId = 12345;

            // Act
            var transaction = _builder
                .SetFrom("01sender")
                .SetTarget("01target")
                .SetAmount("1000")
                .SetTransferId(transferId)
                .Build();

            // Assert
            Assert.IsTrue(transaction.TransferId.HasValue);
            Assert.AreEqual(transferId, transaction.TransferId.Value);
        }

        [Test]
        public void Build_FluentAPI_ShouldAllowChaining()
        {
            // Act
            var transaction = _builder
                .SetFrom("01sender")
                .SetTarget("01target")
                .SetAmount("1000000000")
                .SetGasPrice(1)
                .SetTTL(3600000)
                .SetTransferId(999)
                .Build();

            // Assert
            Assert.IsNotNull(transaction);
            Assert.AreEqual("01sender", transaction.From);
            Assert.AreEqual("01target", transaction.Target);
            Assert.AreEqual("1000000000", transaction.Amount);
            Assert.AreEqual(1, transaction.GasPrice);
            Assert.AreEqual(3600000, transaction.TTL);
            Assert.AreEqual((ulong)999, transaction.TransferId);
        }

        [Test]
        public void Build_ShouldSetTimestamp()
        {
            // Act
            var transaction = _builder
                .SetFrom("01sender")
                .SetTarget("01target")
                .SetAmount("1000")
                .Build();

            // Assert
            Assert.IsNotNull(transaction.Timestamp);
            Assert.IsNotEmpty(transaction.Timestamp);
        }

        [Test]
        public void Build_ShouldSetChainName()
        {
            // Act
            var transaction = _builder
                .SetFrom("01sender")
                .SetTarget("01target")
                .SetAmount("1000")
                .Build();

            // Assert
            Assert.IsNotNull(transaction.ChainName);
            Assert.IsNotEmpty(transaction.ChainName);
        }
    }
}
