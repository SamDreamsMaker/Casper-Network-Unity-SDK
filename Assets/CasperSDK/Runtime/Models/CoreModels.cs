namespace CasperSDK.Models
{
    /// <summary>
    /// Key algorithm enumeration
    /// </summary>
    public enum KeyAlgorithm
    {
        /// <summary>
        /// ED25519 algorithm (recommended)
        /// </summary>
        ED25519,
        
        /// <summary>
        /// SECP256K1 algorithm
        /// </summary>
        SECP256K1
    }

    /// <summary>
    /// Represents a cryptographic key pair
    /// </summary>
    public class KeyPair
    {
        /// <summary>
        /// Public key in hexadecimal format
        /// </summary>
        public string PublicKeyHex { get; set; }

        /// <summary>
        /// Private key in hexadecimal format (should be kept secure)
        /// </summary>
        public string PrivateKeyHex { get; set; }

        /// <summary>
        /// Key algorithm used
        /// </summary>
        public KeyAlgorithm Algorithm { get; set; }

        /// <summary>
        /// Account hash derived from public key
        /// </summary>
        public string AccountHash { get; set; }
    }

    /// <summary>
    /// Represents a Casper account
    /// </summary>
    public class Account
    {
        /// <summary>
        /// Account hash
        /// </summary>
        public string AccountHash { get; set; }

        /// <summary>
        /// Public key
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        /// Main purse URef
        /// </summary>
        public string MainPurse { get; set; }

        /// <summary>
        /// Account balance in motes
        /// </summary>
        public string Balance { get; set; }

        /// <summary>
        /// Associated keys with weights
        /// </summary>
        public AssociatedKey[] AssociatedKeys { get; set; }

        /// <summary>
        /// Action thresholds
        /// </summary>
        public ActionThresholds ActionThresholds { get; set; }
    }

    /// <summary>
    /// Represents an associated key with weight
    /// </summary>
    public class AssociatedKey
    {
        public string AccountHash { get; set; }
        public byte Weight { get; set; }
    }

    /// <summary>
    /// Action thresholds for account operations
    /// </summary>
    public class ActionThresholds
    {
        public byte Deployment { get; set; }
        public byte KeyManagement { get; set; }
    }

    /// <summary>
    /// Represents a transaction/deploy
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// Transaction hash
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Sender's public key
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Recipient's public key (for transfers)
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Transfer amount in motes
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        /// Gas price
        /// </summary>
        public long GasPrice { get; set; }

        /// <summary>
        /// Time-to-live in milliseconds
        /// </summary>
        public long TTL { get; set; }

        /// <summary>
        /// Transfer ID (optional)
        /// </summary>
        public ulong? TransferId { get; set; }

        /// <summary>
        /// Transaction timestamp
        /// </summary>
        public string Timestamp { get; set; }

        /// <summary>
        /// Chain name
        /// </summary>
        public string ChainName { get; set; }

        /// <summary>
        /// Approval signatures
        /// </summary>
        public Approval[] Approvals { get; set; }
    }

    /// <summary>
    /// Represents a transaction approval/signature
    /// </summary>
    public class Approval
    {
        public string Signer { get; set; }
        public string Signature { get; set; }
    }

    /// <summary>
    /// Execution result of a transaction
    /// </summary>
    public class ExecutionResult
    {
        /// <summary>
        /// Transaction hash
        /// </summary>
        public string TransactionHash { get; set; }

        /// <summary>
        /// Block hash where transaction was executed
        /// </summary>
        public string BlockHash { get; set; }

        /// <summary>
        /// Execution status
        /// </summary>
        public ExecutionStatus Status { get; set; }

        /// <summary>
        /// Error message if execution failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gas cost
        /// </summary>
        public long GasCost { get; set; }

        /// <summary>
        /// Execution results
        /// </summary>
        public string[] Transfers { get; set; }
    }

    /// <summary>
    /// Transaction execution status
    /// </summary>
    public enum ExecutionStatus
    {
        Pending,
        Success,
        Failed
    }
}
