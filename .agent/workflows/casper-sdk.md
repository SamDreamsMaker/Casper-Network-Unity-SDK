---
description: Casper Network Unity SDK - Project knowledge and conventions
---

# Casper Network Unity SDK - Project Guide

## ⚠️ IMPORTANT: Keep This Document Updated
After making significant changes to the project structure, deployment process, or conventions, UPDATE THIS FILE to reflect the changes. This ensures future conversations have accurate context.

---

## Project Structure

```
Assets/CasperSDK/
├── Editor/              # Editor tools (DemoSceneCreator - internal only)
├── Runtime/             # Main SDK code
│   ├── Core/            # Configuration, interfaces
│   ├── Models/          # Data models (KeyPair, CLValue, Deploy)
│   ├── Network/         # RPC client, network communication
│   ├── Services/        # Account, Transfer, Deploy services
│   └── Utilities/       # Cryptography, helpers
├── Plugins/             # External DLLs (BouncyCastle)
├── Samples/             # Demo scenes (becomes Samples~ in UPM)
│   └── CasperWalletDemo/
│       ├── Editor/      # Custom inspectors
│       └── *.cs, *.unity
├── Tests/               # Unit tests (excluded from UPM)
└── package.json         # UPM package manifest
```

---

## UPM Package Distribution

### Repositories
- **Source**: `github.com/SamDreamsMaker/Casper-Network-Unity-SDK`
- **UPM Package**: `github.com/SamDreamsMaker/com.caspernetwork.sdk`

### How It Works
1. Push to `main` branch triggers `.github/workflows/publish-upm.yml`
2. Workflow copies `Assets/CasperSDK/*` to UPM repo
3. **Excludes**: Tests/, DemoSceneCreator.cs
4. **Renames**: Samples/ → Samples~ (UPM convention)
5. Force-pushes with tag v{version} from package.json

### Triggering Deployment
Workflow triggers on changes to:
- `Assets/CasperSDK/**`
- `.github/workflows/publish-upm.yml`

---

## Key Conventions

### Samples Structure
- Keep `Samples/` (visible in dev) - workflow renames to `Samples~` on deploy
- Sample scripts in `Samples/CasperWalletDemo/`
- Sample editor scripts in `Samples/CasperWalletDemo/Editor/`

### Things Excluded from UPM Package
- `Tests/` - Unit tests not needed by end users
- `DemoSceneCreator.cs` - Internal dev tool, causes errors if samples not imported

### Custom Editors
- Use `[CustomEditor]` for better Inspector UX
- Example: `CasperWalletDemoControllerEditor.cs` shows "Create Network Config" button

### Cross-Platform Compatibility
- Use reflection to detect optional packages (Input System, URP)
- Example: `Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem")`

---

## Common Tasks

### Regenerate Demo Scene
1. Open Unity in source project (not the package)
2. Window → Casper SDK → Create Demo Scene
3. Commit the updated .unity file

### Test Package Import
1. Clear Unity cache: `%LOCALAPPDATA%\Unity\cache\packages`
2. Create new Unity project
3. Package Manager → Add from Git URL
4. Enter: `https://github.com/SamDreamsMaker/com.caspernetwork.sdk.git`

### Update Version
1. Edit `Assets/CasperSDK/package.json` → `version` field
2. Commit and push to main
3. Workflow auto-creates new tag

---

## Casper Blockchain Specifics

### Key Formats
- **ED25519**: `01` + 32 bytes (64 hex + 2 prefix = 66 chars)
- **SECP256K1**: `02` + 33 bytes compressed point (66 hex + 2 prefix = 68 chars)

### Deploy Serialization
- Binary serialization for hashes (body hash, deploy hash)
- JSON-RPC format for API calls (CLType as objects for complex types)

### Networks
- **Testnet**: chain_name = "casper-test"
- **Mainnet**: chain_name = "casper"

---

## Troubleshooting

### "no such addressable entity" Error
- Account hasn't received any funds yet
- Use faucet (testnet) or transfer from existing account

### Package Cache Issues
- Delete `%LOCALAPPDATA%\Unity\cache\packages`
- Restart Unity

### Workflow Not Triggering
- Check if changed files are in trigger paths
- Verify GitHub Actions secret `UPM_DEPLOY_TOKEN` is valid
