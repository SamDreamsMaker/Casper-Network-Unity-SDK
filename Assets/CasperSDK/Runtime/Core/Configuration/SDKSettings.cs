namespace CasperSDK.Core.Configuration
{
    /// <summary>
    /// Global SDK settings and preferences
    /// </summary>
    public class SDKSettings
    {
        /// <summary>
        /// SDK version
        /// </summary>
        public const string Version = "1.0.0";

        /// <summary>
        /// Default gas price for transactions
        /// </summary>
        public const long DefaultGasPrice = 1;

        /// <summary>
        /// Default gas limit for transactions
        /// </summary>
        public const long DefaultGasLimit = 100000000;

        /// <summary>
        /// Default gas for native CSPR transfers (10,000 motes standard)
        /// </summary>
        public const long DefaultTransferGas = 10000;

        /// <summary>
        /// Minimum balance warning threshold (in motes)
        /// </summary>
        public const long MinBalanceWarningThreshold = 2500000000; // 2.5 CSPR

        /// <summary>
        /// Default transaction time-to-live in milliseconds (1 hour)
        /// </summary>
        public const long DefaultTransactionTTL = 3600000;

        /// <summary>
        /// Polling interval for transaction status checks (in milliseconds)
        /// </summary>
        public const int TransactionPollingInterval = 5000;

        /// <summary>
        /// Maximum transaction polling duration (in milliseconds - 10 minutes)
        /// </summary>
        public const int MaxTransactionPollingDuration = 600000;
    }
}
