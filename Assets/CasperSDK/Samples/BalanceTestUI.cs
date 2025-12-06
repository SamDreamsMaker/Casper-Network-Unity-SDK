using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CasperSDK.Core;
using CasperSDK.Core.Configuration;
using System.Threading.Tasks;

namespace CasperSDK.Samples
{
    /// <summary>
    /// Simple UI test for checking account balance on Casper testnet.
    /// </summary>
    public class BalanceTestUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_InputField accountAddressInput;
        [SerializeField] private Button getBalanceButton;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private TMP_Text statusText;

        [Header("SDK Configuration")]
        [SerializeField] private NetworkConfig networkConfig;

        private bool isProcessing = false;

        private void Start()
        {
            Debug.Log("[BalanceTest] === START METHOD CALLED ===");
            
            // Ensure we have an EventSystem for UI interactions
            var existingEventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (existingEventSystem == null)
            {
                var eventSystemObj = new GameObject("EventSystem");
                var eventSystem = eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                
                // Add Standalone Input Module for UI interactions
                // This works with both old and new Input System
                var inputModule = eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                
                Debug.Log("[BalanceTest] EventSystem created with StandaloneInputModule");
            }
            else
            {
                Debug.Log("[BalanceTest] EventSystem already exists");
                // Check if it has an input module
                var inputModule = existingEventSystem.GetComponent<UnityEngine.EventSystems.BaseInputModule>();
                if (inputModule == null)
                {
                    Debug.LogWarning("[BalanceTest] EventSystem exists but has no InputModule! Adding one...");
                    existingEventSystem.gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }
            }

            // Initialize UI
            if (resultText != null)
            {
                resultText.text = "Enter a Casper account address and click 'Get Balance'";
                Debug.Log("[BalanceTest] ResultText initialized");
            }
            else
            {
                Debug.LogError("[BalanceTest] ResultText is NULL!");
            }

            if (statusText != null)
            {
                statusText.text = "Ready";
                statusText.color = Color.white;
                Debug.Log("[BalanceTest] StatusText initialized");
            }
            else
            {
                Debug.LogError("[BalanceTest] StatusText is NULL!");
            }

            // Setup button listener
            if (getBalanceButton != null)
            {
                getBalanceButton.onClick.AddListener(OnGetBalanceClicked);
                Debug.Log("[BalanceTest] Button listener added successfully!");
            }
            else
            {
                Debug.LogError("[BalanceTest] GetBalanceButton is NULL! Button won't work!");
            }

            // Check input field
            if (accountAddressInput != null)
            {
                Debug.Log("[BalanceTest] AccountAddressInput assigned correctly");
            }
            else
            {
                Debug.LogError("[BalanceTest] AccountAddressInput is NULL!");
            }

            // Initialize SDK
            InitializeSDK();
        }

        private void InitializeSDK()
        {
            try
            {
                UpdateStatus("Initializing SDK...", Color.yellow);

                // Load config from Resources if not assigned
                if (networkConfig == null)
                {
                    networkConfig = Resources.Load<NetworkConfig>("TestnetConfig");
                }

                // If still null, create a default testnet config
                if (networkConfig == null)
                {
                    Debug.LogWarning("[BalanceTest] Config not found, creating default testnet config");
                    networkConfig = ScriptableObject.CreateInstance<NetworkConfig>();
                }

                // Initialize the SDK
                CasperSDKManager.Instance.Initialize(networkConfig);

                UpdateStatus("SDK Initialized - Ready", Color.green);
                Debug.Log($"[BalanceTest] SDK initialized successfully on {networkConfig.NetworkType}");
                Debug.Log($"[BalanceTest] Using RPC URL: {networkConfig.RpcUrl}");
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"SDK Initialization Failed: {ex.Message}", Color.red);
                Debug.LogError($"[BalanceTest] Failed to initialize SDK: {ex}");
            }
        }

        private async void OnGetBalanceClicked()
        {
            Debug.Log("[BalanceTest] *** BUTTON CLICKED! ***");
            
            if (isProcessing)
            {
                Debug.LogWarning("[BalanceTest] Already processing a request");
                return;
            }

            string address = accountAddressInput != null ? accountAddressInput.text.Trim() : "";
            Debug.Log($"[BalanceTest] Address from input: '{address}'");

            if (string.IsNullOrWhiteSpace(address))
            {
                UpdateStatus("Please enter an account address", Color.red);
                UpdateResult("Error: No address provided");
                return;
            }

            await GetBalanceAsync(address);
        }

        private async Task GetBalanceAsync(string accountAddress)
        {
            isProcessing = true;

            try
            {
                UpdateStatus($"Fetching balance for {accountAddress.Substring(0, 10)}...", Color.yellow);
                UpdateResult("Loading...");

                // Disable button during processing
                if (getBalanceButton != null)
                {
                    getBalanceButton.interactable = false;
                }

                // Get balance from SDK
                var accountService = CasperSDKManager.Instance.AccountService;
                string balance = await accountService.GetBalanceAsync(accountAddress);

                // Convert motes to CSPR (1 CSPR = 1,000,000,000 motes)
                if (decimal.TryParse(balance, out decimal balanceInMotes))
                {
                    decimal balanceInCSPR = balanceInMotes / 1_000_000_000m;
                    
                    UpdateStatus("Balance retrieved successfully", Color.green);
                    UpdateResult($"<b>Balance:</b>\n{balanceInCSPR:N9} CSPR\n({balanceInMotes:N0} motes)");
                }
                else
                {
                    UpdateStatus("Balance retrieved", Color.green);
                    UpdateResult($"<b>Balance:</b> {balance} motes");
                }

                Debug.Log($"[BalanceTest] Balance retrieved: {balance} motes");
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"Error: {ex.Message}", Color.red);
                UpdateResult($"<color=red><b>Error:</b></color>\n{ex.Message}\n\nPlease check:\n- Account address is valid\n- Network connection is active\n- Testnet is accessible");
                Debug.LogError($"[BalanceTest] Error getting balance: {ex}");
            }
            finally
            {
                isProcessing = false;

                // Re-enable button
                if (getBalanceButton != null)
                {
                    getBalanceButton.interactable = true;
                }
            }
        }

        private void UpdateStatus(string message, Color color)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = color;
            }
        }

        private void UpdateResult(string message)
        {
            if (resultText != null)
            {
                resultText.text = message;
            }
        }

        private void OnDestroy()
        {
            // Cleanup button listener
            if (getBalanceButton != null)
            {
                getBalanceButton.onClick.RemoveListener(OnGetBalanceClicked);
            }
        }
    }
}
