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
| 🔐 **Real Cryptography** | ED25519 & SECP256K1 with BouncyCastle |
| 💸 **CSPR Transfers** | Send CSPR tokens with one method call |
| 📜 **Smart Contracts** | Deploy WASM and call contract methods |
| 🎨 **NFT Support** | CEP-78 standard: mint, transfer, burn |
| 📡 **Event Streaming** | Real-time SSE for deploy/block events |
| 🔒 **Secure Storage** | AES-256 encrypted key storage |
| 👛 **Multi-Wallet** | Manage multiple accounts |
| 🔗 **Blockchain Integration** | Connect to Mainnet or Testnet |
| 📄 **Key Import/Export** | PEM format for Casper Wallet/Signer |
| 🎮 **Unity Optimized** | Async/await, main thread handling |

---

## 📦 Installation

### Option 1: Unity Package Manager (Recommended)

1. Open **Window → Package Manager**
2. Click **+ → Add package from git URL**
3. Enter:
```
https://github.com/SamDreamsMaker/com.caspernetwork.sdk.git
```

### Option 2: From Source Repo

For development or to access sample scenes:
```
https://github.com/SamDreamsMaker/Casper-Network-Unity-SDK.git?path=Assets/CasperSDK
```

### Dependencies

The SDK includes:
- ✅ BouncyCastle.Crypto.dll (cryptography - included in Plugins/)
- ✅ Newtonsoft.Json (auto-installed via UPM)

---

## 🚀 Quick Start

### Demo Scene (Easiest Way)

1. **Window → Casper SDK → Create Demo Scene**
2. Play the generated `CasperWalletDemo` scene
3. Click **Import Keys** to load your Casper Wallet PEM file
4. Use the UI buttons for all operations

### Basic Usage

```csharp
using CasperSDK.Utilities.Cryptography;
using CasperSDK.Services.Transfer;

// Generate a new key pair
var keyPair = CasperKeyGenerator.GenerateED25519();
Debug.Log($"Public Key: {keyPair.PublicKeyHex}");
Debug.Log($"Account Hash: {keyPair.AccountHash}");

// Transfer CSPR
var transferService = new TransferService(networkClient, config);
var result = await transferService.TransferAsync(
    senderKeyPair,
    recipientPublicKey,
    TransferService.CsprToMotes(10m) // 10 CSPR
);
```

### Import Keys from Casper Wallet

```csharp
// Import PEM file exported from Casper Wallet
var keyPair = KeyExporter.ImportFromPemFile("path/to/secret_key.pem");

// Export for Casper Wallet
KeyExporter.ExportToPemFiles(keyPair, "output/path");
```

### Deploy Smart Contract

```csharp
var contractService = new ContractService(networkClient, config);

// Deploy WASM
var result = await contractService.DeployContractAsync(
    wasmBytes,
    args,
    senderKeyPair,
    "50000000000" // 50 CSPR payment
);

// Call contract method
var callResult = await contractService.CallContractByHashAsync(
    contractHash,
    "transfer",
    runtimeArgs,
    senderKeyPair
);
```

---

## 📚 Documentation

### Services Overview

| Service | Description |
|---------|-------------|
| **AccountService** | Balance queries, key generation, account import |
| **TransferService** | High-level CSPR transfers |
| **ContractService** | WASM deployment, contract calls, state queries |
| **CEP78Service** | NFT mint, transfer, burn, metadata queries |
| **EventStreamingService** | Real-time SSE events (deploys, blocks) |
| **SecureKeyStorage** | AES-256 encrypted key storage |
| **WalletManager** | Multi-account management |
| **BlockService** | Block queries by hash/height |
| **NetworkInfoService** | Node status, peers, chainspec |

### Cryptography

| Class | Purpose |
|-------|---------|
| **CasperKeyGenerator** | Generate ED25519/SECP256K1 keys |
| **KeyExporter** | PEM import/export |
| **DeploySigner** | Sign deploys with private key |
| **CLValueBuilder** | Build runtime arguments |
| **CryptoHelper** | Blake2b, hex conversion, validation |

### Configuration

Create via **Window → Casper SDK → Settings** or:

```
Right-click in Project → Create → CasperSDK → Network Config
```

| Property | Description | Default |
|----------|-------------|---------|
| `NetworkType` | Mainnet, Testnet, Custom | Testnet |
| `RpcUrl` | JSON-RPC endpoint | testnet node |
| `EnableLogging` | Debug logs | true |

---

## 🎯 Examples

### CasperWalletDemo UI

| Button | Action |
|--------|--------|
| Generate Account | Create new ED25519 key pair |
| Import Keys | Load PEM from Documents/CasperKeys |
| Refresh Balance | Query CSPR balance |
| Copy Address | Copy public key to clipboard |
| Export Keys | Save PEM to Documents/CasperKeys |
| Open Faucet | Get free testnet CSPR |
| Send Transaction | Transfer CSPR to recipient |

### Workflow for Testing

1. **Casper Wallet → Settings → Download Secret Key** → Save PEM
2. Copy PEM to `Documents/CasperKeys/`
3. In Unity: **Window → Casper SDK → Create Demo Scene**
4. **Play** → Click **Import Keys** → Keys loaded
5. **Refresh Balance** → See your CSPR
6. Enter recipient + amount → **Send Transaction**

---

## 🗂️ Project Structure

```
Assets/CasperSDK/
├── Editor/                 # Settings window, DemoSceneCreator
├── Plugins/                # BouncyCastle.Crypto.dll
├── Runtime/
│   ├── Core/               # Configuration, Interfaces
│   ├── Models/             # Deploy, KeyPair, CLValue
│   ├── Network/            # RPC client
│   ├── Services/
│   │   ├── Account/        # Balance, keys
│   │   ├── Contract/       # WASM, calls
│   │   ├── Deploy/         # Builder, Signer, CLValueBuilder
│   │   ├── Events/         # SSE streaming
│   │   ├── NFT/            # CEP-78 support
│   │   ├── Storage/        # Secure key storage
│   │   ├── Transfer/       # CSPR transfers
│   │   └── Wallet/         # Multi-account
│   ├── Utilities/
│   │   └── Cryptography/   # Keys, hashing, PEM import/export
│   └── Examples/           # TestnetDemo, BasicSDKExample
├── Samples/                # CasperWalletDemo scene & controller
└── Tests/                  # Unit tests (12 test files)
```

---

## 🔧 Technical Details

### Cryptography (BouncyCastle)

- **ED25519**: Key generation, signing, verification
- **SECP256K1**: Key generation, ECDSA signing
- **Blake2b-256**: Deploy hash, account hash
- **PEM**: Import/export for wallets

### Transaction Flow

1. **Build** → DeployBuilder with fluent API
2. **Hash** → Blake2b-256 body + header hash
3. **Sign** → ED25519 or SECP256K1 signature
4. **Submit** → account_put_deploy RPC

---

## 📄 License

MIT License - see [LICENSE](LICENSE)

---

## 🤝 Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push (`git push origin feature/amazing`)
5. Open Pull Request

---

## 📞 Support

- 🐛 [Report Bug](https://github.com/SamDreamsMaker/Casper-Network-Unity-SDK/issues)
- 💡 [Request Feature](https://github.com/SamDreamsMaker/Casper-Network-Unity-SDK/issues)

---

<div align="center">

**Built with ❤️ for the Casper ecosystem**

</div>
