using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CasperSDK.Core.Configuration;
using CasperSDK.Core.Interfaces;
using CasperSDK.Network.Clients;
using CasperSDK.Services.Account;
using CasperSDK.Services.Transfer;
using CasperSDK.Utilities.Cryptography;
using CasperSDK.Models;

namespace CasperSDK.Samples
{
    /// <summary>
    /// Controller for the Casper Wallet Demo scene.
    /// Wires up UI elements with SDK functionality.
    /// </summary>
    public class CasperWalletDemoController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private NetworkConfig _networkConfig;

        // UI References (found automatically)
        private TMP_Text _balanceValue;
        private TMP_Text _addressValue;
        private TMP_Text _transferStatus;
        private TMP_InputField _recipientInput;
        private TMP_InputField _amountInput;
        
        private Button _generateBtn;
        private Button _refreshBtn;
        private Button _copyBtn;
        private Button _exportBtn;
        private Button _faucetBtn;
        private Button _sendBtn;

        // Services
        private INetworkClient _networkClient;
        private AccountService _accountService;
        private TransferService _transferService;
        private KeyPair _currentKeyPair;

        private void Awake()
        {
            FindUIElements();
            InitializeServices();
            SetupButtonListeners();
        }

        private void Start()
        {
            // Auto-generate an account on start
            GenerateNewAccount();
        }

        private void FindUIElements()
        {
            // Find text elements
            _balanceValue = FindComponentInChildren<TMP_Text>("BalanceValue");
            _addressValue = FindComponentInChildren<TMP_Text>("AddressValue");
            _transferStatus = FindComponentInChildren<TMP_Text>("TransferStatus");
            
            // Find inputs
            _recipientInput = FindComponentInChildren<TMP_InputField>("RecipientInput");
            _amountInput = FindComponentInChildren<TMP_InputField>("AmountInput");
            
            // Find buttons
            _generateBtn = FindComponentInChildren<Button>("GenerateBtn");
            _refreshBtn = FindComponentInChildren<Button>("RefreshBtn");
            _copyBtn = FindComponentInChildren<Button>("CopyBtn");
            _exportBtn = FindComponentInChildren<Button>("ExportBtn");
            _faucetBtn = FindComponentInChildren<Button>("FaucetBtn");
            _sendBtn = FindComponentInChildren<Button>("SendBtn");
        }

        private T FindComponentInChildren<T>(string name) where T : Component
        {
            var allComponents = FindObjectsByType<T>(FindObjectsSortMode.None);
            foreach (var comp in allComponents)
            {
                if (comp.gameObject.name == name)
                    return comp;
            }
            return null;
        }

        private void InitializeServices()
        {
            if (_networkConfig == null)
            {
                _networkConfig = ScriptableObject.CreateInstance<NetworkConfig>();
            }
            
            _networkClient = NetworkClientFactory.CreateClient(_networkConfig);
            _accountService = new AccountService(_networkClient, _networkConfig);
            _transferService = new TransferService(_networkClient, _networkConfig);
            
            Debug.Log("[CasperDemo] Services initialized");
        }

        private void SetupButtonListeners()
        {
            _generateBtn?.onClick.AddListener(GenerateNewAccount);
            _refreshBtn?.onClick.AddListener(RefreshBalance);
            _copyBtn?.onClick.AddListener(CopyAddress);
            _exportBtn?.onClick.AddListener(ExportKeys);
            _faucetBtn?.onClick.AddListener(OpenFaucet);
            _sendBtn?.onClick.AddListener(SendTransaction);
        }

        #region Actions

        private void GenerateNewAccount()
        {
            _currentKeyPair = CasperKeyGenerator.GenerateED25519();
            UpdateAddressDisplay();
            SetBalance("0.00");
            SetStatus("New account generated!", Color.green);
            
            Debug.Log($"[CasperDemo] Generated: {_currentKeyPair.PublicKeyHex}");
        }

        private async void RefreshBalance()
        {
            if (_currentKeyPair == null)
            {
                SetStatus("No account. Generate first!", Color.yellow);
                return;
            }

            SetStatus("Fetching balance...", Color.white);
            
            try
            {
                var balance = await _accountService.GetBalanceAsync(_currentKeyPair.PublicKeyHex);
                var cspr = TransferService.MotesToCspr(balance);
                SetBalance($"{cspr:N2}");
                SetStatus($"Balance updated", Color.green);
            }
            catch (Exception ex)
            {
                SetBalance("0.00");
                SetStatus($"Not found on chain (use faucet first)", Color.yellow);
                Debug.LogWarning($"[CasperDemo] Balance check: {ex.Message}");
            }
        }

        private void CopyAddress()
        {
            if (_currentKeyPair == null) return;
            
            GUIUtility.systemCopyBuffer = _currentKeyPair.PublicKeyHex;
            SetStatus("Address copied to clipboard!", Color.green);
        }

        private void ExportKeys()
        {
            if (_currentKeyPair == null)
            {
                SetStatus("No account to export!", Color.yellow);
                return;
            }

            try
            {
                var path = System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),
                    "CasperKeys");
                    
                KeyExporter.ExportToPemFiles(_currentKeyPair, path, "casper_demo");
                SetStatus($"Keys exported to Documents/CasperKeys", Color.green);
                
                #if UNITY_EDITOR || UNITY_STANDALONE_WIN
                System.Diagnostics.Process.Start("explorer.exe", path);
                #endif
            }
            catch (Exception ex)
            {
                SetStatus($"Export failed: {ex.Message}", Color.red);
            }
        }

        private void OpenFaucet()
        {
            Application.OpenURL("https://testnet.cspr.live/tools/faucet");
            SetStatus("Faucet opened. Paste your address there!", Color.cyan);
        }

        private async void SendTransaction()
        {
            if (_currentKeyPair == null)
            {
                SetStatus("No account!", Color.red);
                return;
            }

            var recipient = _recipientInput?.text?.Trim();
            var amountText = _amountInput?.text?.Trim();

            if (string.IsNullOrEmpty(recipient))
            {
                SetStatus("Enter recipient address", Color.yellow);
                return;
            }

            if (!decimal.TryParse(amountText, out var amount) || amount <= 0)
            {
                SetStatus("Enter valid amount", Color.yellow);
                return;
            }

            SetStatus("Sending transaction...", Color.white);
            _sendBtn.interactable = false;

            try
            {
                var motes = TransferService.CsprToMotes(amount);
                var result = await _transferService.TransferAsync(_currentKeyPair, recipient, motes);

                if (result.Success)
                {
                    SetStatus($"Sent! Hash: {result.DeployHash.Substring(0, 16)}...", Color.green);
                    _recipientInput.text = "";
                    _amountInput.text = "";
                    
                    // Refresh balance after a delay
                    Invoke(nameof(RefreshBalance), 3f);
                }
                else
                {
                    SetStatus($"Failed: {result.ErrorMessage}", Color.red);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}", Color.red);
            }
            finally
            {
                _sendBtn.interactable = true;
            }
        }

        #endregion

        #region UI Helpers

        private void UpdateAddressDisplay()
        {
            if (_addressValue != null && _currentKeyPair != null)
            {
                var key = _currentKeyPair.PublicKeyHex;
                if (key.Length > 20)
                {
                    _addressValue.text = $"{key.Substring(0, 10)}...{key.Substring(key.Length - 8)}";
                }
                else
                {
                    _addressValue.text = key;
                }
            }
        }

        private void SetBalance(string cspr)
        {
            if (_balanceValue != null)
            {
                _balanceValue.text = $"{cspr} CSPR";
            }
        }

        private void SetStatus(string message, Color color)
        {
            if (_transferStatus != null)
            {
                _transferStatus.text = message;
                _transferStatus.color = color;
            }
            
            Debug.Log($"[CasperDemo] {message}");
        }

        #endregion
    }
}
