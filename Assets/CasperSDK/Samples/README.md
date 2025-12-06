# Casper Network Balance Test Sample

Ce sample démontre comment utiliser le Casper Network Unity SDK pour récupérer le solde d'un compte sur le testnet.

## Contenu

- **BalanceTestScene.unity** : Scène Unity avec l'interface utilisateur de test
- **BalanceTestUI.cs** : Script MonoBehaviour qui gère l'UI et les appels au SDK

## Comment utiliser

### 1. Ouvrir la scène

Dans Unity, ouvrez la scène `Assets/CasperSDK/Samples/BalanceTestScene.unity`

### 2. Lancer le test

1. Cliquez sur le bouton **Play** dans Unity
2. Entrez une adresse de compte public Casper dans le champ de saisie
3. Cliquez sur le bouton **Get Balance**
4. Le solde s'affichera en CSPR et en motes

### 3. Exemple d'adresse de test

Voici un exemple d'adresse publique au format Casper (hex avec préfixe algorithme) :

```
0203a8eb50fc1d6e50cc02f96e7de5c0a29f7de2ec7093f4d73aab3dd2a35aff88a5
```
- **NetworkConfig** : Configuration testnet (créée automatiquement si absente)
- **AccountService** : Service pour les opérations liées aux comptes
- **JsonRpcClient** : Client HTTP pour les appels RPC

## Code exemple

```csharp
// Initialiser le SDK
var config = Resources.Load<NetworkConfig>("TestnetConfig");
CasperSDKManager.Instance.Initialize(config);

// Récupérer le solde
var accountService = CasperSDKManager.Instance.AccountService;
string balance = await accountService.GetBalanceAsync(publicKey);

// Le solde est retourné en motes (1 CSPR = 1,000,000,000 motes)
```

## Troubleshooting
## Prochaines étapes

Pour aller plus loin avec le SDK :

- Consultez `Assets/CasperSDK/README.md` pour la documentation complète
- Explorez les autres services disponibles (TransactionService, etc.)
- Créez votre propre application en utilisant ce sample comme référence
