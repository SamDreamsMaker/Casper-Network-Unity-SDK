using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using CasperSDK.Models;
using CasperSDK.Utilities.Cryptography;
using CasperSDK.Core.Configuration;
using Org.BouncyCastle.Crypto.Digests;

namespace CasperSDK.Services.Deploy
{
    /// <summary>
    /// Builder for creating Casper Network deploys.
    /// Implements the Builder pattern for constructing complex deploy objects.
    /// </summary>
    public class DeployBuilder
    {
        private string _senderPublicKey;
        private string _chainName = "casper-test";
        private long _gasPrice = 1;
        private long _ttl = 1800000; // 30 minutes
        private string[] _dependencies = Array.Empty<string>();
        private ExecutableDeployItem _payment;
        private ExecutableDeployItem _session;
        private DateTime? _timestamp;

        /// <summary>
        /// Sets the sender's public key
        /// </summary>
        public DeployBuilder SetSender(string publicKey)
        {
            if (string.IsNullOrWhiteSpace(publicKey))
                throw new ArgumentException("Sender public key cannot be null or empty");
            
            _senderPublicKey = publicKey;
            return this;
        }

        /// <summary>
        /// Sets the chain name (default: casper-test)
        /// </summary>
        public DeployBuilder SetChainName(string chainName)
        {
            if (string.IsNullOrWhiteSpace(chainName))
                throw new ArgumentException("Chain name cannot be null or empty");
            
            _chainName = chainName;
            return this;
        }

        /// <summary>
        /// Sets the gas price (default: 1)
        /// </summary>
        public DeployBuilder SetGasPrice(long gasPrice)
        {
            if (gasPrice <= 0)
                throw new ArgumentException("Gas price must be positive");
            
            _gasPrice = gasPrice;
            return this;
        }

        /// <summary>
        /// Sets the time-to-live in milliseconds (default: 30 minutes)
        /// </summary>
        public DeployBuilder SetTTL(long ttlMs)
        {
            if (ttlMs <= 0)
                throw new ArgumentException("TTL must be positive");
            
            _ttl = ttlMs;
            return this;
        }

        /// <summary>
        /// Sets the timestamp (default: now)
        /// </summary>
        public DeployBuilder SetTimestamp(DateTime timestamp)
        {
            _timestamp = timestamp;
            return this;
        }

        /// <summary>
        /// Sets deploy dependencies
        /// </summary>
        public DeployBuilder SetDependencies(string[] deployHashes)
        {
            _dependencies = deployHashes ?? Array.Empty<string>();
            return this;
        }

        /// <summary>
        /// Sets the payment (standard payment with gas amount)
        /// </summary>
        public DeployBuilder SetStandardPayment(string amount)
        {
            if (string.IsNullOrWhiteSpace(amount))
                throw new ArgumentException("Payment amount cannot be null or empty");

            _payment = new ExecutableDeployItem
            {
                Type = "ModuleBytes",
                ModuleBytes = "", // Empty for standard payment
                Args = new[]
                {
                    new RuntimeArg
                    {
                        Name = "amount",
                        Value = CLValueBuilder.U512(amount)
                    }
                }
            };
            return this;
        }

        /// <summary>
        /// Sets the session as a native transfer
        /// </summary>
        public DeployBuilder SetTransferSession(string targetPublicKey, string amount, ulong? transferId = null)
        {
            if (string.IsNullOrWhiteSpace(targetPublicKey))
                throw new ArgumentException("Target public key cannot be null or empty");
            if (string.IsNullOrWhiteSpace(amount))
                throw new ArgumentException("Amount cannot be null or empty");

            var args = new List<RuntimeArg>
            {
                new RuntimeArg { Name = "amount", Value = CLValueBuilder.U512(amount) },
                new RuntimeArg { Name = "target", Value = CLValueBuilder.PublicKey(targetPublicKey) }
            };

            if (transferId.HasValue)
            {
                args.Add(new RuntimeArg { Name = "id", Value = CLValueBuilder.OptionU64(transferId.Value) });
            }
            else
            {
                args.Add(new RuntimeArg { Name = "id", Value = CLValueBuilder.OptionNone() });
            }

            _session = new ExecutableDeployItem
            {
                Type = "Transfer",
                Args = args.ToArray()
            };
            return this;
        }

        /// <summary>
        /// Sets the session as a stored contract call
        /// </summary>
        public DeployBuilder SetContractSession(string contractHash, string entryPoint, RuntimeArg[] args)
        {
            _session = new ExecutableDeployItem
            {
                Type = "StoredContractByHash",
                ContractHash = contractHash,
                EntryPoint = entryPoint,
                Args = args ?? Array.Empty<RuntimeArg>()
            };
            return this;
        }

        /// <summary>
        /// Sets the session as WASM module bytes
        /// </summary>
        public DeployBuilder SetWasmSession(byte[] wasmBytes, RuntimeArg[] args)
        {
            _session = new ExecutableDeployItem
            {
                Type = "ModuleBytes",
                ModuleBytes = CryptoHelper.BytesToHex(wasmBytes),
                Args = args ?? Array.Empty<RuntimeArg>()
            };
            return this;
        }

        /// <summary>
        /// Builds the deploy without signing
        /// </summary>
        public Models.Deploy Build()
        {
            Validate();

            var timestamp = _timestamp ?? DateTime.UtcNow;
            var timestampStr = timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            // Calculate body hash
            var bodyHash = CalculateBodyHash(_payment, _session);

            // Create header
            var header = new DeployHeader
            {
                Account = _senderPublicKey,
                Timestamp = timestampStr,
                TTL = _ttl,
                GasPrice = _gasPrice,
                BodyHash = bodyHash,
                Dependencies = _dependencies,
                ChainName = _chainName
            };

            // Calculate deploy hash from header
            var deployHash = CalculateDeployHash(header);

            return new Models.Deploy
            {
                Hash = deployHash,
                Header = header,
                Payment = _payment,
                Session = _session,
                Approvals = Array.Empty<DeployApproval>()
            };
        }

        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(_senderPublicKey))
                throw new InvalidOperationException("Sender public key is required");
            if (_payment == null)
                throw new InvalidOperationException("Payment is required");
            if (_session == null)
                throw new InvalidOperationException("Session is required");
        }

        /// <summary>
        /// Calculates the body hash (Blake2b-256 of serialized payment + session)
        /// </summary>
        private string CalculateBodyHash(ExecutableDeployItem payment, ExecutableDeployItem session)
        {
            // Serialize payment and session
            var paymentBytes = SerializeExecutableDeployItem(payment);
            var sessionBytes = SerializeExecutableDeployItem(session);

            // Combine
            var combined = new byte[paymentBytes.Length + sessionBytes.Length];
            Buffer.BlockCopy(paymentBytes, 0, combined, 0, paymentBytes.Length);
            Buffer.BlockCopy(sessionBytes, 0, combined, paymentBytes.Length, sessionBytes.Length);

            // Hash with Blake2b-256
            return HashBlake2b256(combined);
        }

        /// <summary>
        /// Calculates the deploy hash (Blake2b-256 of serialized header)
        /// </summary>
        private string CalculateDeployHash(DeployHeader header)
        {
            var headerBytes = SerializeDeployHeader(header);
            return HashBlake2b256(headerBytes);
        }

        /// <summary>
        /// Computes Blake2b-256 hash
        /// </summary>
        private string HashBlake2b256(byte[] data)
        {
            var blake2b = new Blake2bDigest(256);
            blake2b.BlockUpdate(data, 0, data.Length);
            var hash = new byte[32];
            blake2b.DoFinal(hash, 0);
            return CryptoHelper.BytesToHex(hash);
        }

        /// <summary>
        /// Serializes a deploy header to bytes (simplified Casper serialization)
        /// </summary>
        private byte[] SerializeDeployHeader(DeployHeader header)
        {
            var parts = new List<byte>();

            // Account (public key with length prefix)
            var accountBytes = CryptoHelper.HexToBytes(header.Account);
            parts.AddRange(BitConverter.GetBytes((uint)accountBytes.Length));
            parts.AddRange(accountBytes);

            // Timestamp (as milliseconds since epoch)
            var timestamp = DateTime.Parse(header.Timestamp).ToUniversalTime();
            var epochMs = (long)(timestamp - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            parts.AddRange(BitConverter.GetBytes(epochMs));

            // TTL
            parts.AddRange(BitConverter.GetBytes(header.TTL));

            // Gas price
            parts.AddRange(BitConverter.GetBytes(header.GasPrice));

            // Body hash
            var bodyHashBytes = CryptoHelper.HexToBytes(header.BodyHash);
            parts.AddRange(bodyHashBytes);

            // Dependencies count and hashes
            parts.AddRange(BitConverter.GetBytes((uint)header.Dependencies.Length));
            foreach (var dep in header.Dependencies)
            {
                parts.AddRange(CryptoHelper.HexToBytes(dep));
            }

            // Chain name (length-prefixed string)
            var chainNameBytes = Encoding.UTF8.GetBytes(header.ChainName);
            parts.AddRange(BitConverter.GetBytes((uint)chainNameBytes.Length));
            parts.AddRange(chainNameBytes);

            return parts.ToArray();
        }

        /// <summary>
        /// Serializes an executable deploy item to bytes
        /// </summary>
        private byte[] SerializeExecutableDeployItem(ExecutableDeployItem item)
        {
            var parts = new List<byte>();

            // Type tag
            byte typeTag = item.Type switch
            {
                "ModuleBytes" => 0,
                "StoredContractByHash" => 1,
                "StoredContractByName" => 2,
                "StoredVersionedContractByHash" => 3,
                "StoredVersionedContractByName" => 4,
                "Transfer" => 5,
                _ => 0
            };
            parts.Add(typeTag);

            // Serialize based on type
            switch (item.Type)
            {
                case "ModuleBytes":
                    var moduleBytes = string.IsNullOrEmpty(item.ModuleBytes) 
                        ? Array.Empty<byte>() 
                        : CryptoHelper.HexToBytes(item.ModuleBytes);
                    parts.AddRange(BitConverter.GetBytes((uint)moduleBytes.Length));
                    parts.AddRange(moduleBytes);
                    parts.AddRange(SerializeRuntimeArgs(item.Args));
                    break;

                case "Transfer":
                    parts.AddRange(SerializeRuntimeArgs(item.Args));
                    break;

                case "StoredContractByHash":
                    var contractBytes = CryptoHelper.HexToBytes(item.ContractHash);
                    parts.AddRange(contractBytes);
                    var epBytes = Encoding.UTF8.GetBytes(item.EntryPoint ?? "");
                    parts.AddRange(BitConverter.GetBytes((uint)epBytes.Length));
                    parts.AddRange(epBytes);
                    parts.AddRange(SerializeRuntimeArgs(item.Args));
                    break;

                default:
                    // Simplified for other types
                    parts.AddRange(SerializeRuntimeArgs(item.Args));
                    break;
            }

            return parts.ToArray();
        }

        /// <summary>
        /// Serializes runtime arguments
        /// </summary>
        private byte[] SerializeRuntimeArgs(RuntimeArg[] args)
        {
            var parts = new List<byte>();
            args = args ?? Array.Empty<RuntimeArg>();

            // Args count
            parts.AddRange(BitConverter.GetBytes((uint)args.Length));

            foreach (var arg in args)
            {
                // Name (length-prefixed)
                var nameBytes = Encoding.UTF8.GetBytes(arg.Name ?? "");
                parts.AddRange(BitConverter.GetBytes((uint)nameBytes.Length));
                parts.AddRange(nameBytes);

                // Value bytes
                var valueBytes = CryptoHelper.HexToBytes(arg.Value?.Bytes ?? "");
                parts.AddRange(BitConverter.GetBytes((uint)valueBytes.Length));
                parts.AddRange(valueBytes);
            }

            return parts.ToArray();
        }
    }
}
