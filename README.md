# 🚀 Casper Network Unity SDK

<div align="center">

[![Unity](https://img.shields.io/badge/Unity-2022.3+-000000?style=for-the-badge&logo=unity)](https://unity.com/)
[![Casper](https://img.shields.io/badge/Casper-Network-FF0012?style=for-the-badge)](https://casper.network/)
[![License](https://img.shields.io/badge/License-MIT-blue?style=for-the-badge)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-Standard%202.1-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)

**A modern, production-ready Unity SDK for integrating with the Casper blockchain network.**

[Features](#-features) • [Installation](#-installation) • [Quick Start](#-quick-start) • [Documentation](#-documentation) • [Examples](#-examples)

</div>

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🔗 **Real Blockchain Integration** | Connect to Casper Mainnet or Testnet with real RPC calls |
| 💰 **Balance Queries** | Fetch account balances in CSPR and motes |
| 🧱 **Block Explorer** | Query blocks by height, hash, or get the latest block |
| 📡 **Network Status** | Get node status, peers list, and chainspec info |
| 📜 **Deploy Tracking** | Query and monitor deploy execution status |
| 🔐 **Key Generation** | Generate ED25519 and SECP256K1 key pairs |
| 🏗️ **Transaction Builder** | Fluent API for building transactions |
| 🧪 **Mock Network** | Offline development with mock responses |
| 🎮 **Unity Optimized** | Proper main thread handling, async/await support |

---

## 📦 Installation

### Option 1: Unity Package Manager (Recommended)

1. Open **Window → Package Manager**
2. Click **+ → Add package from git URL**
3. Enter:
```
https://github.com/SamDreamsMaker/Casper-Network-Unity-SDK.git
```

### Option 2: Manual Installation

1. Download or clone this repository
2. Copy the `Assets/CasperSDK` folder into your Unity project's `Assets/` directory

### Dependencies

The SDK automatically includes:
- ✅ Newtonsoft.Json (via Unity Package Manager)
- ✅ TextMeshPro (for UI examples)

---

## 🚀 Quick Start

### 1. Initialize the SDK

```csharp
using CasperSDK.Core;
using CasperSDK.Core.Configuration;

// Load configuration from Resources
var config = Resources.Load<NetworkConfig>("TestnetConfig");
CasperSDKManager.Instance.Initialize(config);
```

### 2. Get Account Balance

```csharp
var accountService = CasperSDKManager.Instance.AccountService;
string balance = await accountService.GetBalanceAsync(publicKey);
Debug.Log($"Balance: {balance} motes");
```

### 3. Query Latest Block

```csharp
var blockService = CasperSDKManager.Instance.BlockService;
var block = await blockService.GetLatestBlockAsync();
Debug.Log($"Block height: {block.Header.Height}");
```

### 4. Check Network Status

```csharp
var networkService = CasperSDKManager.Instance.NetworkInfoService;
var status = await networkService.GetStatusAsync();
Debug.Log($"Chain: {status.ChainspecName}, Peers: {status.Peers.Length}");
```

---

## 📚 Documentation

### Services Overview

| Service | Methods | Description |
|---------|---------|-------------|
| **AccountService** | `GetBalanceAsync`, `GetAccountAsync`, `GenerateKeyPairAsync`, `ImportAccountAsync` | Account management and balance queries |
| **BlockService** | `GetLatestBlockAsync`, `GetBlockByHashAsync`, `GetBlockByHeightAsync`, `GetStateRootHashAsync` | Blockchain block queries |
| **NetworkInfoService** | `GetStatusAsync`, `GetPeersAsync`, `GetChainspecAsync` | Network and node information |
| **DeployService** | `GetDeployAsync`, `GetDeployStatusAsync`, `SubmitDeployAsync` | Deploy tracking and submission |
| **TransactionService** | `CreateTransactionBuilder`, `SubmitTransactionAsync`, `EstimateGasAsync` | Transaction building and submission |
| **StateService** | `QueryGlobalStateAsync`, `GetDictionaryItemAsync`, `GetDictionaryItemByNameAsync` | Global state and dictionary queries |
| **ValidatorService** | `GetAuctionInfoAsync`, `GetValidatorsAsync`, `GetValidatorByKeyAsync` | Validator and staking information |


### Configuration

Create a `NetworkConfig` ScriptableObject:

```
Right-click in Project → Create → CasperSDK → Network Config
```

| Property | Description | Default |
|----------|-------------|---------|
| `NetworkType` | Mainnet, Testnet, or Custom | Testnet |
| `RpcUrl` | JSON-RPC endpoint URL | `https://node.testnet.casper.network/rpc` |
| `EnableLogging` | Debug logging | true |
| `UseMockNetwork` | Use mock responses for offline dev | false |

---

## 🎯 Examples

### Balance Test Scene

Open `Assets/CasperSDK/Samples/BalanceTestScene.unity` to see a working example:

1. Enter a Casper public key (hex format with algorithm prefix)
2. Click "Get Balance"
3. See real testnet balance displayed

### Example Public Key Format

```
0203ca1f9573d3452e45dfe176903bcbe72d22fcfee53401abbe4f3a4eff0f0db2c3
```
- `02` = SECP256K1 algorithm prefix
- `03ca1f...` = compressed public key

---

## 🗂️ Project Structure

```
Assets/CasperSDK/
├── Editor/                 # Unity Editor tools
├── Resources/              # ScriptableObject configs
├── Runtime/
│   ├── Core/               # SDK Manager, Interfaces, Exceptions
│   ├── Models/             # Data models (Account, Transaction, KeyPair...)
│   │   └── RPC/            # RPC response models
│   ├── Network/            # RPC client, Mock client
│   ├── Services/           # All 7 services
│   │   ├── Account/
│   │   ├── Block/
│   │   ├── Deploy/
│   │   ├── Network/
│   │   ├── State/
│   │   ├── Transaction/
│   │   └── Validator/
│   └── Unity/              # Main thread dispatcher
├── Samples/                # Example scenes and scripts
└── Tests/                  # Unit and Integration tests
```

---

## 🔧 Development

### Branch Strategy

| Branch | Purpose |
|--------|---------|
| `main` | Stable releases |
| `develop` | Active development |

### Building

1. Open the project in Unity 2022.3+
2. Ensure no compilation errors
3. Run tests: **Window → General → Test Runner**

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📞 Support

- 🐛 [Report a Bug](https://github.com/SamDreamsMaker/Casper-Network-Unity-SDK/issues)
- 💡 [Request a Feature](https://github.com/SamDreamsMaker/Casper-Network-Unity-SDK/issues)
- 📧 Contact: [@SamDreamsMaker](https://github.com/SamDreamsMaker)

---

<div align="center">

**Built with ❤️ for the Casper ecosystem**

</div>
