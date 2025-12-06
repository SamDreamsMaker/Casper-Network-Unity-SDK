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

> **Note** : Pour tester avec une adresse valide contenant des fonds :
> - Visitez [cspr.live](https://testnet.cspr.live/)
> - Créez un compte via le Casper Wallet
> - Utilisez le faucet testnet pour obtenir des tokens de test

## Fonctionnalités démontrées

- ✅ Initialisation du SDK avec configuration testnet
- ✅ Appel RPC `state_get_account_info` pour obtenir le purse URef
- ✅ Appel RPC `state_get_balance` pour récupérer le solde
- ✅ Gestion des erreurs et affichage des messages
- ✅ Conversion automatique motes ↔ CSPR
- ✅ Interface utilisateur simple et claire

## Architecture

Le test utilise les composants suivants du SDK :

- **CasperSDKManager** : Point d'entrée singleton du SDK
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

### Le SDK ne s'initialise pas

- Vérifiez que `TestnetConfig.asset` existe dans `Assets/CasperSDK/Resources/`
- Le SDK créera automatiquement une configuration par défaut si le fichier est absent

### Erreur de connexion réseau

- Vérifiez votre connexion Internet
- Le testnet Casper doit être accessible à l'adresse : `http://52.35.59.254:7777/rpc`
- Vérifiez les logs Unity pour plus de détails

### Adresse invalide

- L'adresse doit être au format hexadécimal avec préfixe d'algorithme (ex: `01` pour ED25519, `02` pour SECP256K1)
- L'adresse doit avoir une longueur valide (généralement 66 caractères)

## Prochaines étapes

Pour aller plus loin avec le SDK :

- Consultez `Assets/CasperSDK/README.md` pour la documentation complète
- Explorez les autres services disponibles (TransactionService, etc.)
- Créez votre propre application en utilisant ce sample comme référence
