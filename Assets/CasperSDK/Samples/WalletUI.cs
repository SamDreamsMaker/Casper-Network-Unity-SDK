using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CasperSDK.Core;
using CasperSDK.Core.Configuration;
using CasperSDK.Core.Interfaces;
using CasperSDK.Network.Clients;
using CasperSDK.Services.Account;
using CasperSDK.Services.Transfer;
using CasperSDK.Services.Wallet;
using CasperSDK.Utilities.Cryptography;

namespace CasperSDK.Samples
{
    /// <summary>
    /// Complete wallet UI demonstrating all SDK features.
    /// Attach to a Canvas with the required UI components.
    /// </summary>
    public class WalletUI : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private NetworkConfig _networkConfig;

        [Header("Password Panel")]
        [SerializeField] private GameObject _passwordPanel;
        [SerializeField] private TMP_InputField _passwordInput;
        [SerializeField] private Button _unlockButton;
        [SerializeField] private Button _createWalletButton;

        [Header("Main Panel")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private TMP_Text _balanceText;
        [SerializeField] private TMP_Text _accountLabelText;
        [SerializeField] private TMP_Text _addressText;
        [SerializeField] private Button _copyAddressButton;
        [SerializeField] private Button _refreshBalanceButton;
        [SerializeField] private Button _lockButton;

        [Header("Account Selector")]
        [SerializeField] private TMP_Dropdown _accountDropdown;
        [SerializeField] private Button _addAccountButton;

        [Header("Transfer Panel")]
        [SerializeField] private TMP_InputField _recipientInput;
        [SerializeField] private TMP_InputField _amountInput;
        [SerializeField] private Button _sendButton;
        [SerializeField] private TMP_Text _transferStatusText;

        [Header("Activity Log")]
        [SerializeField] private TMP_Text _logText;
        [SerializeField] private ScrollRect _logScrollRect;

        // Services
        private WalletManager _walletManager;
        private INetworkClient _networkClient;
        private AccountService _accountService;
        private TransferService _transferService;

        private void Start()
        {
            InitializeServices();
            SetupUI();
            ShowPasswordPanel();
        }

        private void InitializeServices()
        {
            // Create network client
            var config = _networkConfig ?? ScriptableObject.CreateInstance<NetworkConfig>();
            _networkClient = NetworkClientFactory.CreateClient(config);
            _accountService = new AccountService(_networkClient, config);
            _transferService = new TransferService(_networkClient, config);
            _walletManager = new WalletManager(true);

            // Subscribe to wallet events
            _walletManager.OnActiveAccountChanged += OnActiveAccountChanged;
            _walletManager.OnAccountsChanged += RefreshAccountDropdown;

            Log("Casper SDK initialized");
        }

        private void SetupUI()
        {
            // Password panel buttons
            _unlockButton?.onClick.AddListener(OnUnlockClicked);
            _createWalletButton?.onClick.AddListener(OnCreateWalletClicked);

            // Main panel buttons
            _copyAddressButton?.onClick.AddListener(OnCopyAddressClicked);
            _refreshBalanceButton?.onClick.AddListener(OnRefreshBalanceClicked);
            _lockButton?.onClick.AddListener(OnLockClicked);
            _addAccountButton?.onClick.AddListener(OnAddAccountClicked);
            _sendButton?.onClick.AddListener(OnSendClicked);

            // Account dropdown
            _accountDropdown?.onValueChanged.AddListener(OnAccountSelected);
        }

        #region Panel Management

        private void ShowPasswordPanel()
        {
            _passwordPanel?.SetActive(true);
            _mainPanel?.SetActive(false);
        }

        private void ShowMainPanel()
        {
            _passwordPanel?.SetActive(false);
            _mainPanel?.SetActive(true);
        }

        #endregion

        #region Button Handlers

        private void OnUnlockClicked()
        {
            var password = _passwordInput?.text;
            if (string.IsNullOrEmpty(password))
            {
                Log("Please enter a password");
                return;
            }

            try
            {
                _walletManager.Unlock(password);
                
                if (_walletManager.AccountCount == 0)
                {
                    // First time - create an account
                    _walletManager.CreateAccount("Main Account");
                    Log("Created new wallet with Main Account");
                }

                ShowMainPanel();
                RefreshAccountDropdown();
                Log($"Wallet unlocked - {_walletManager.AccountCount} account(s)");
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }

        private void OnCreateWalletClicked()
        {
            var password = _passwordInput?.text;
            if (string.IsNullOrEmpty(password) || password.Length < 8)
            {
                Log("Password must be at least 8 characters");
                return;
            }

            try
            {
                _walletManager.Unlock(password);
                _walletManager.CreateAccount("Main Account");
                
                ShowMainPanel();
                RefreshAccountDropdown();
                Log("New wallet created successfully!");
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }

        private void OnLockClicked()
        {
            _walletManager.Lock();
            _passwordInput.text = "";
            ShowPasswordPanel();
            Log("Wallet locked");
        }

        private void OnCopyAddressClicked()
        {
            var address = _walletManager.ActiveAccount?.PublicKey;
            if (!string.IsNullOrEmpty(address))
            {
                GUIUtility.systemCopyBuffer = address;
                Log("Address copied to clipboard");
            }
        }

        private async void OnRefreshBalanceClicked()
        {
            if (_walletManager.ActiveAccount == null) return;

            try
            {
                _balanceText.text = "Loading...";
                var balance = await _accountService.GetBalanceAsync(_walletManager.ActiveAccount.PublicKey);
                var cspr = TransferService.MotesToCspr(balance);
                _balanceText.text = $"{cspr:N2} CSPR";
                _walletManager.ActiveAccount.Balance = balance;
                Log($"Balance updated: {cspr:N2} CSPR");
            }
            catch (Exception ex)
            {
                _balanceText.text = "0 CSPR";
                Log($"Balance check failed: {ex.Message}");
            }
        }

        private void OnAddAccountClicked()
        {
            try
            {
                var accountNumber = _walletManager.AccountCount + 1;
                var account = _walletManager.CreateAccount($"Account {accountNumber}");
                RefreshAccountDropdown();
                Log($"Created: {account.Label}");
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }

        private void OnAccountSelected(int index)
        {
            if (index >= 0 && index < _walletManager.AccountCount)
            {
                var account = _walletManager.Accounts[index];
                _walletManager.SetActiveAccount(account.Label);
            }
        }

        private async void OnSendClicked()
        {
            var recipient = _recipientInput?.text?.Trim();
            var amountText = _amountInput?.text?.Trim();

            if (string.IsNullOrEmpty(recipient))
            {
                SetTransferStatus("Enter recipient address", Color.yellow);
                return;
            }

            if (!decimal.TryParse(amountText, out var amount) || amount <= 0)
            {
                SetTransferStatus("Enter valid amount", Color.yellow);
                return;
            }

            if (_walletManager.ActiveAccount?.KeyPair == null)
            {
                SetTransferStatus("No active account", Color.red);
                return;
            }

            try
            {
                SetTransferStatus("Sending...", Color.white);
                _sendButton.interactable = false;

                var motes = TransferService.CsprToMotes(amount);
                var result = await _transferService.TransferAsync(
                    _walletManager.ActiveAccount.KeyPair,
                    recipient,
                    motes);

                if (result.Success)
                {
                    SetTransferStatus($"Sent! Hash: {result.DeployHash.Substring(0, 16)}...", Color.green);
                    Log($"Transfer successful: {amount} CSPR to {recipient.Substring(0, 16)}...");
                    _recipientInput.text = "";
                    _amountInput.text = "";
                    OnRefreshBalanceClicked();
                }
                else
                {
                    SetTransferStatus($"Failed: {result.ErrorMessage}", Color.red);
                    Log($"Transfer failed: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                SetTransferStatus($"Error: {ex.Message}", Color.red);
                Log($"Transfer error: {ex.Message}");
            }
            finally
            {
                _sendButton.interactable = true;
            }
        }

        #endregion

        #region UI Updates

        private void OnActiveAccountChanged(WalletAccount account)
        {
            if (account == null)
            {
                _accountLabelText.text = "No Account";
                _addressText.text = "";
                _balanceText.text = "0 CSPR";
                return;
            }

            _accountLabelText.text = account.Label;
            _addressText.text = account.ShortPublicKey;
            OnRefreshBalanceClicked();
        }

        private void RefreshAccountDropdown()
        {
            if (_accountDropdown == null) return;

            _accountDropdown.ClearOptions();
            var options = new System.Collections.Generic.List<string>();

            foreach (var account in _walletManager.Accounts)
            {
                options.Add($"{account.Label} ({account.ShortPublicKey})");
            }

            _accountDropdown.AddOptions(options);

            // Set current selection
            var activeIndex = 0;
            for (int i = 0; i < _walletManager.Accounts.Count; i++)
            {
                if (_walletManager.Accounts[i].Label == _walletManager.ActiveAccount?.Label)
                {
                    activeIndex = i;
                    break;
                }
            }
            _accountDropdown.SetValueWithoutNotify(activeIndex);
        }

        private void SetTransferStatus(string message, Color color)
        {
            if (_transferStatusText != null)
            {
                _transferStatusText.text = message;
                _transferStatusText.color = color;
            }
        }

        private void Log(string message)
        {
            Debug.Log($"[WalletUI] {message}");
            
            if (_logText != null)
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                _logText.text += $"[{timestamp}] {message}\n";
                
                // Auto-scroll to bottom
                if (_logScrollRect != null)
                {
                    Canvas.ForceUpdateCanvases();
                    _logScrollRect.verticalNormalizedPosition = 0f;
                }
            }
        }

        #endregion

        private void OnDestroy()
        {
            _walletManager?.Lock();
        }
    }
}
