using System;
using System.IO;
using System.Text;
using UnityEngine;
using CasperSDK.Models;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace CasperSDK.Utilities.Cryptography
{
    /// <summary>
    /// Exports and imports keys in PEM format for compatibility with Casper Signer.
    /// </summary>
    public static class KeyExporter
    {
        /// <summary>
        /// Exports a key pair to PEM format files (public and private keys)
        /// </summary>
        /// <param name="keyPair">Key pair to export</param>
        /// <param name="directoryPath">Directory to save files</param>
        /// <param name="filePrefix">Prefix for file names (default: "casper_key")</param>
        public static void ExportToPemFiles(KeyPair keyPair, string directoryPath, string filePrefix = "casper_key")
        {
            if (keyPair == null)
                throw new ArgumentNullException(nameof(keyPair));

            // Create directory if it doesn't exist
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var privateKeyPath = Path.Combine(directoryPath, $"{filePrefix}_secret_key.pem");
            var publicKeyPath = Path.Combine(directoryPath, $"{filePrefix}_public_key.pem");

            // Export private key
            var privatePem = ExportPrivateKeyToPem(keyPair);
            File.WriteAllText(privateKeyPath, privatePem);

            // Export public key  
            var publicPem = ExportPublicKeyToPem(keyPair);
            File.WriteAllText(publicKeyPath, publicPem);

            Debug.Log($"[CasperSDK] Keys exported to:");
            Debug.Log($"  Private: {privateKeyPath}");
            Debug.Log($"  Public: {publicKeyPath}");
        }

        /// <summary>
        /// Exports private key to PEM string
        /// </summary>
        public static string ExportPrivateKeyToPem(KeyPair keyPair)
        {
            var privateKeyBytes = CryptoHelper.HexToBytes(keyPair.PrivateKeyHex);

            if (keyPair.Algorithm == KeyAlgorithm.ED25519)
            {
                return ExportED25519PrivateKeyPem(privateKeyBytes);
            }
            else
            {
                return ExportSECP256K1PrivateKeyPem(privateKeyBytes);
            }
        }

        /// <summary>
        /// Exports public key to PEM string
        /// </summary>
        public static string ExportPublicKeyToPem(KeyPair keyPair)
        {
            // Remove algorithm prefix (01 or 02)
            var publicKeyHex = keyPair.PublicKeyHex.Substring(2);
            var publicKeyBytes = CryptoHelper.HexToBytes(publicKeyHex);

            if (keyPair.Algorithm == KeyAlgorithm.ED25519)
            {
                return ExportED25519PublicKeyPem(publicKeyBytes);
            }
            else
            {
                return ExportSECP256K1PublicKeyPem(publicKeyBytes);
            }
        }

        /// <summary>
        /// Exports key pair to Casper Signer compatible format (secret_key.pem)
        /// This is the format used by casper-client and Casper Signer
        /// </summary>
        public static string ExportForCasperSigner(KeyPair keyPair)
        {
            return ExportPrivateKeyToPem(keyPair);
        }

        #region ED25519 Export

        private static string ExportED25519PrivateKeyPem(byte[] privateKeyBytes)
        {
            var privateKey = new Ed25519PrivateKeyParameters(privateKeyBytes, 0);
            
            using (var stringWriter = new StringWriter())
            {
                var pemWriter = new PemWriter(stringWriter);
                pemWriter.WriteObject(privateKey);
                return stringWriter.ToString();
            }
        }

        private static string ExportED25519PublicKeyPem(byte[] publicKeyBytes)
        {
            var publicKey = new Ed25519PublicKeyParameters(publicKeyBytes, 0);
            
            using (var stringWriter = new StringWriter())
            {
                var pemWriter = new PemWriter(stringWriter);
                pemWriter.WriteObject(publicKey);
                return stringWriter.ToString();
            }
        }

        #endregion

        #region SECP256K1 Export

        private static string ExportSECP256K1PrivateKeyPem(byte[] privateKeyBytes)
        {
            var curve = ECNamedCurveTable.GetByName("secp256k1");
            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);
            var d = new BigInteger(1, privateKeyBytes);
            var privateKey = new ECPrivateKeyParameters(d, domainParams);

            using (var stringWriter = new StringWriter())
            {
                var pemWriter = new PemWriter(stringWriter);
                pemWriter.WriteObject(privateKey);
                return stringWriter.ToString();
            }
        }

        private static string ExportSECP256K1PublicKeyPem(byte[] publicKeyBytes)
        {
            var curve = ECNamedCurveTable.GetByName("secp256k1");
            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);
            var q = curve.Curve.DecodePoint(publicKeyBytes);
            var publicKey = new ECPublicKeyParameters(q, domainParams);

            using (var stringWriter = new StringWriter())
            {
                var pemWriter = new PemWriter(stringWriter);
                pemWriter.WriteObject(publicKey);
                return stringWriter.ToString();
            }
        }

        #endregion

        #region Import

        /// <summary>
        /// Imports a key pair from a PEM file
        /// </summary>
        public static KeyPair ImportFromPemFile(string pemFilePath, KeyAlgorithm algorithm = KeyAlgorithm.ED25519)
        {
            if (!File.Exists(pemFilePath))
                throw new FileNotFoundException("PEM file not found", pemFilePath);

            var pemContent = File.ReadAllText(pemFilePath);
            return ImportFromPem(pemContent, algorithm);
        }

        /// <summary>
        /// Imports a key pair from PEM string
        /// </summary>
        public static KeyPair ImportFromPem(string pemContent, KeyAlgorithm algorithm = KeyAlgorithm.ED25519)
        {
            using (var stringReader = new StringReader(pemContent))
            {
                var pemReader = new PemReader(stringReader);
                var keyObject = pemReader.ReadObject();

                if (keyObject is Ed25519PrivateKeyParameters ed25519Private)
                {
                    var publicKey = ed25519Private.GeneratePublicKey();
                    return new KeyPair
                    {
                        PrivateKeyHex = CryptoHelper.BytesToHex(ed25519Private.GetEncoded()),
                        PublicKeyHex = "01" + CryptoHelper.BytesToHex(publicKey.GetEncoded()),
                        Algorithm = KeyAlgorithm.ED25519,
                        AccountHash = CryptoHelper.GenerateAccountHash("01" + CryptoHelper.BytesToHex(publicKey.GetEncoded()))
                    };
                }
                else if (keyObject is ECPrivateKeyParameters ecPrivate)
                {
                    var curve = ECNamedCurveTable.GetByName("secp256k1");
                    var publicKeyPoint = curve.G.Multiply(ecPrivate.D);
                    var publicKeyBytes = publicKeyPoint.GetEncoded(true);

                    return new KeyPair
                    {
                        PrivateKeyHex = CryptoHelper.BytesToHex(ecPrivate.D.ToByteArrayUnsigned()),
                        PublicKeyHex = "02" + CryptoHelper.BytesToHex(publicKeyBytes),
                        Algorithm = KeyAlgorithm.SECP256K1,
                        AccountHash = CryptoHelper.GenerateAccountHash("02" + CryptoHelper.BytesToHex(publicKeyBytes))
                    };
                }

                throw new ArgumentException("Unsupported key format in PEM file");
            }
        }

        #endregion
    }
}
