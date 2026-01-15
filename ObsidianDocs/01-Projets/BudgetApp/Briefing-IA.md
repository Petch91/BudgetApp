# Briefing IA - BudgetApp

> Copie-colle ce fichier quand tu demarres une session avec Claude Code ou Claude.ai

---

## Le projet en 3 lignes
Application de gestion de budget personnel en Blazor Server (.NET 10).
Gere les depenses fixes recurrentes (loyer, abonnements) avec echeances et rappels.
Gere aussi les transactions variables (revenus/depenses ponctuelles) avec suivi mensuel.

## Stack
- **Blazor** : Server (InteractiveServer)
- **.NET** : 10.0
- **SQL Server** + EF Core 10.0.1
- **UI** : Blazor.Bootstrap 3.5.0
- **Packages cles** : FluentValidation, FluentResults, Serilog, Swashbuckle

## Structure solution
```
BudgetApp/
├── Entities/              # Domaine (Models, DTOs, Forms)
├── Application/           # Services metier + DbContext
├── BudgetApp.Shared/      # Composants Blazor partages
└── Front_BudgetApp/       # App Blazor Server + API REST
```

## Decisions architecturales importantes
1. **Clean Architecture** : 4 projets separes par responsabilite
2. **TPH** : Heritage Transaction -> DepenseFixe/TransactionVariable
3. **FluentResults** : Gestion d'erreurs sans exceptions
4. **Minimal APIs** : Endpoints REST groupes par domaine
5. **BackgroundService** : Scheduler pour maintenance echeances/rappels

## Ou j'en suis
**Fonctionnalites terminees** :
- CRUD Depenses fixes avec echeances et rappels
- CRUD Transactions variables (revenus/depenses)
- CRUD Categories
- Statistiques mensuelles
- Navigation par mois
- Page unifiee avec onglets

**En cours** :
[A remplir]

**Prochaine etape** :
[A remplir]

## Ma question/besoin du moment
[A remplir avant de coller a l'IA]

## Docs completes
Pour le detail complet : voir `ObsidianDocs/01-Projets/BudgetApp/README.md`

---

## Points d'attention connus
- `Application.csproj` contient encore `<RootNamespace>Datas</RootNamespace>`
- Interface `ITranscationService` avec faute de frappe
- Pas de tests unitaires
- Pas d'authentification
