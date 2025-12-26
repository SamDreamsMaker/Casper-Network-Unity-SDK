using System;
using NUnit.Framework;
using CasperSDK.Core.Configuration;
using CasperSDK.Services.Events;
using UnityEngine;

namespace CasperSDK.Tests.Unit
{
    /// <summary>
    /// Unit tests for EventStreamingService
    /// </summary>
    [TestFixture]
    public class EventStreamingServiceTests
    {
        private EventStreamingService _service;
        private NetworkConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<NetworkConfig>();
            // EnableLogging is readonly, will use default value
            _service = new EventStreamingService(_config);
        }

        [TearDown]
        public void TearDown()
        {
            _service?.Dispose();
        }

        [Test]
        public void Constructor_WithNullConfig_UsesDefaults()
        {
            // Act
            var service = new EventStreamingService(null);

            // Assert
            Assert.IsNotNull(service);
            Assert.IsFalse(service.IsConnected);
            service.Dispose();
        }

        [Test]
        public void IsConnected_Initially_IsFalse()
        {
            // Assert
            Assert.IsFalse(_service.IsConnected);
        }

        [Test]
        public void Stop_WhenNotConnected_DoesNotThrow()
        {
            // Act & Assert - should not throw
            Assert.DoesNotThrow(() => _service.Stop());
        }

        [Test]
        public void Dispose_CanBeCalled_MultipleTimes()
        {
            // Act & Assert - should not throw
            Assert.DoesNotThrow(() =>
            {
                _service.Dispose();
                _service.Dispose();
            });
        }

        [Test]
        public void DeployEvent_Properties_AreSettable()
        {
            // Arrange & Act
            var evt = new DeployEvent
            {
                DeployHash = "test_hash",
                Account = "test_account",
                Success = true,
                ErrorMessage = null,
                Cost = "1000000",
                Timestamp = DateTime.UtcNow
            };

            // Assert
            Assert.AreEqual("test_hash", evt.DeployHash);
            Assert.AreEqual("test_account", evt.Account);
            Assert.IsTrue(evt.Success);
            Assert.AreEqual("1000000", evt.Cost);
        }

        [Test]
        public void BlockEvent_Properties_AreSettable()
        {
            // Arrange & Act
            var evt = new BlockEvent
            {
                BlockHash = "block_hash",
                Height = 12345,
                Proposer = "validator_key",
                DeployCount = 5,
                Timestamp = DateTime.UtcNow
            };

            // Assert
            Assert.AreEqual("block_hash", evt.BlockHash);
            Assert.AreEqual(12345, evt.Height);
            Assert.AreEqual("validator_key", evt.Proposer);
            Assert.AreEqual(5, evt.DeployCount);
        }

        [Test]
        public void EventChannel_HasCorrectValues()
        {
            // Assert
            Assert.AreEqual(0, (int)EventChannel.Main);
            Assert.AreEqual(1, (int)EventChannel.Deploys);
            Assert.AreEqual(2, (int)EventChannel.Sigs);
        }

        [Test]
        public void OnConnected_Event_CanBeSubscribed()
        {
            // Arrange
            var eventFired = false;
            _service.OnConnected += () => eventFired = true;

            // Assert - just verify subscription works
            Assert.IsFalse(eventFired); // Not connected yet
        }

        [Test]
        public void OnDisconnected_Event_CanBeSubscribed()
        {
            // Arrange
            var eventFired = false;
            _service.OnDisconnected += () => eventFired = true;

            // Act
            _service.Stop(); // This triggers disconnect

            // Assert
            Assert.IsTrue(eventFired);
        }

        [Test]
        public void OnError_Event_CanBeSubscribed()
        {
            // Arrange
            string errorMessage = null;
            _service.OnError += (msg) => errorMessage = msg;

            // Assert - just verify subscription works
            Assert.IsNull(errorMessage);
        }

        [Test]
        public void OnDeployAccepted_Event_CanBeSubscribed()
        {
            // Arrange
            DeployEvent receivedEvent = null;
            _service.OnDeployAccepted += (evt) => receivedEvent = evt;

            // Assert
            Assert.IsNull(receivedEvent);
        }

        [Test]
        public void OnDeployProcessed_Event_CanBeSubscribed()
        {
            // Arrange
            DeployEvent receivedEvent = null;
            _service.OnDeployProcessed += (evt) => receivedEvent = evt;

            // Assert
            Assert.IsNull(receivedEvent);
        }

        [Test]
        public void OnBlockAdded_Event_CanBeSubscribed()
        {
            // Arrange
            BlockEvent receivedEvent = null;
            _service.OnBlockAdded += (evt) => receivedEvent = evt;

            // Assert
            Assert.IsNull(receivedEvent);
        }
    }
}
