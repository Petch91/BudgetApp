# BudgetApp

## Objectif du projet
**Probleme resolu** : Gestion des depenses personnelles avec suivi des depenses fixes recurrentes et des transactions variables (revenus/depenses ponctuelles)
**Type d'application** : Blazor Server avec API REST integree
**Utilisateurs cibles** : Usage personnel pour le suivi budgetaire

## Stack technique actuelle

### Frontend
- **Blazor Server** (.NET 10.0)
- **Blazor.Bootstrap** 3.5.0 - Composants UI Bootstrap pour Blazor
- **Theme Dark Mode** personnalise (bleu-gris et orange)
- Composants principaux :
  - `Home.razor` - Tableau de bord avec totaux du mois en cours
  - `DepenseFixe_C.razor` - Grille des depenses fixes avec statistiques
  - `TransactionVariable_C.razor` - Gestion des transactions variables
  - `Rapport_C.razor` - Rapport mensuel avec filtres par categorie
  - `Categories_C.razor` - Gestion des categories
  - `CategorieForm_C.razor` - Formulaire de categorie

### Backend
- **.NET 10.0** avec ASP.NET Core Minimal APIs
- Services :
  - `DepenseFixeService` - CRUD + generation echeances
  - `TransactionService` - CRUD transactions variables
  - `CategorieService` - CRUD categories
  - `RapportService` - Generation des rapports mensuels
  - `AuthService` - Authentification JWT (login, refresh, logout)
  - `UserService` - Gestion des utilisateurs
  - `DepenseFixeScheduler` - Service de maintenance planifie (BackgroundService)

### Base de donnees
- **SQL Server** avec Entity Framework Core 10.0.1
- Connexion : `192.168.50.48` / Database `BudgetDB`
- Migrations : InitialAfterAddUser, AddUserIdToTransactions, AddEtalonnage

### Packages NuGet notables
| Package | Version | Role |
|---------|---------|------|
| Blazor.Bootstrap | 3.5.0 | Composants UI (Grid, Modal, Alert, etc.) |
| FluentValidation | 12.1.1 | Validation declarative des formulaires |
| FluentResults | 4.0.0 | Gestion d'erreurs sans exceptions |
| Serilog | 4.3.1-dev | Logging structure |
| Swashbuckle | 10.0.1 | Documentation API Swagger |
| JWT Bearer | 10.0.2 | Authentification API |

## Architecture detectee

### Structure de la solution
```
BudgetApp/
├── Entities/                         # Domaine (Models, DTOs, Forms, Interfaces)
│   ├── Domain/
│   │   ├── Models/                   # Transaction, DepenseFixe, Categorie, User, etc.
│   │   └── Interfaces/               # IModel, ITransaction
│   └── Contracts/
│       ├── Dtos/                     # DepenseFixeDto, CategorieDto, AuthenticatedUserDto, etc.
│       ├── Forms/                    # DepenseFixeForm, CategorieForm, LoginForm, etc.
│       └── Validations/              # Validators FluentValidation
│
├── Application/                      # Logique metier & persistance
│   ├── Interfaces/                   # IDepenseFixeService, ICategorieService, IAuthService, etc.
│   ├── Services/                     # Implementations des services
│   ├── Services/Securite/            # PasswordManager, JwtTokenGenerator
│   ├── Persistence/
│   │   ├── MyDbContext.cs            # DbContext EF Core
│   │   └── Migrations/               # Migrations EF Core
│   ├── Projections/                  # Expressions LINQ pour mapping efficace
│   ├── Mappers/                      # Extensions de mapping DTO
│   └── Tools/                        # ResultEnum
│
├── BudgetApp.Shared/                 # Composants Blazor partages
│   ├── Components/
│   │   ├── Transactions/             # DepenseFixe_C, TransactionVariable_C
│   │   ├── Rapport/                  # Rapport_C
│   │   └── Categories/               # Categories_C, CategorieForm_C
│   ├── Interfaces/Http/              # IHttpDepenseFixe, IHttpCategorie, IHttpTransaction, IHttpRapport
│   ├── Services/Notifications/       # IAppToastService
│   └── Tools/                        # SerilogConfig, Icones
│
└── Front_BudgetApp/
    ├── Front_BudgetApp/              # Application Blazor Server principale
    │   ├── Components/
    │   │   ├── Pages/                # DepenseFixePage, CategoriesPage, Home, Login
    │   │   └── Layout/               # MainLayout, LoginLayout, NavMenu, RedirectToLogin
    │   ├── Api/Endpoints/            # Minimal APIs REST
    │   ├── Services/                 # Services HTTP frontend + HttpErrorHelper
    │   ├── Services/Securite/        # AuthStateService, CustomAuthStateProvider
    │   └── Tools/                    # DepenseFixeScheduler
    └── Front_BudgetApp.Client/       # Projet client (WebAssembly si besoin)
```

### Organisation du code
**Modeles** : `Entities/Domain/Models/` - Transaction, DepenseFixe, TransactionVariable, Categorie, User, Rappel, DepenseDueDate, RefreshToken
**Services** : `Application/Services/` - DepenseFixeService, TransactionService, CategorieService, AuthService
**Pages Blazor** : `Front_BudgetApp/Components/Pages/` - DepenseFixePage (avec onglets), CategoriesPage, Home, Login
**Composants reutilisables** : `BudgetApp.Shared/Components/` - DepenseFixe_C, TransactionVariable_C, Categories_C, Rapport_C

### Patterns identifies
- [x] **Clean Architecture** : Separation Entities/Application/Presentation
- [x] **Service Pattern** : Services metier injectes via DI
- [x] **Repository-like** : Services encapsulent l'acces DbContext
- [x] **Dependency Injection** : Tous services enregistres dans Program.cs
- [x] **Result Pattern** : FluentResults pour gestion d'erreurs
- [x] **TPH (Table Per Hierarchy)** : Heritage Transaction -> DepenseFixe/TransactionVariable
- [x] **Minimal APIs** : Endpoints REST groupes par domaine
- [x] **JWT Bearer** : Authentification API avec refresh proactif
- [ ] CQRS : Non

## Entites et relations

```mermaid
erDiagram
    User ||--o{ Transaction : possede
    User ||--o{ RefreshToken : possede
    Categorie ||--o{ Transaction : contient
    Transaction ||--|| DepenseFixe : "herite (TPH)"
    Transaction ||--|| TransactionVariable : "herite (TPH)"
    DepenseFixe ||--o{ DepenseDueDate : possede
    DepenseFixe ||--o{ Rappel : possede

    User {
        int Id PK
        string Username
        string Email
        string PasswordHash
        bool IsActive
        datetime LastLoginAt
        datetime CreatedAt
        datetime UpdatedAt
    }

    Categorie {
        int Id PK
        string Name
        string Icon
        datetime CreatedAt
        datetime UpdatedAt
    }

    Transaction {
        int Id PK
        string Intitule
        decimal Montant
        int CategorieId FK
        int UserId FK
        datetime CreatedAt
        datetime UpdatedAt
        string TransactionTable "Discriminateur TPH"
    }

    DepenseFixe {
        Frequence Frequence
        bool EstDomiciliee
        int ReminderDaysBefore
        bool IsEchelonne
        int NombreEcheances
        decimal MontantParEcheance
        int EcheancesRestantes
    }

    TransactionVariable {
        datetime Date
        TransactionType TransactionType
    }

    DepenseDueDate {
        int Id PK
        datetime Date
        int DepenseId FK
        datetime CreatedAt
        datetime UpdatedAt
    }

    Rappel {
        int Id PK
        int DepenseFixeId FK
        datetime RappelDate
        bool Vu
        datetime CreatedAt
        datetime UpdatedAt
    }

    RefreshToken {
        int Id PK
        string Token
        datetime ExpiresAt
        bool IsRevoked
        int UserId FK
    }
```

## Configuration et startup

**Program.cs** :
- Services enregistres :
  - `IDepenseFixeService` -> `DepenseFixeService`
  - `ITranscationService` -> `TransactionService`
  - `ICategorieService` -> `CategorieService`
  - `IRapportService` -> `RapportService`
  - `IAuthService` -> `AuthService`
  - `IUserService` -> `UserService`
  - `IHttpDepenseFixe` -> `DepenseFixeFrontService`
  - `IHttpCategorie` -> `CategorieFrontService`
  - `IHttpTransaction` -> `TransactionFrontService`
  - `IHttpRapport` -> `RapportFrontService`
  - `IAppToastService` -> `AppToastService`
  - `AuthStateService` - Session frontend avec timer refresh
  - `CustomAuthStateProvider` - AuthenticationStateProvider
- Background Service : `DepenseFixeScheduler` (maintenance horaire)
- Middleware : StaticFiles, Antiforgery, Swagger (dev), JWT Bearer
- HttpClient : Configure pour appeler l'API locale

## Etat actuel du developpement

**Fonctionnalites implementees** :
- [x] CRUD Depenses fixes avec echeances et rappels
- [x] CRUD Transactions variables (revenus/depenses)
- [x] CRUD Categories
- [x] Statistiques mensuelles (totaux, solde)
- [x] Alertes rappels urgents (< 3 jours)
- [x] Navigation par mois pour transactions
- [x] Scheduler de maintenance automatique
- [x] **Tableau de bord** - Totaux revenus/depenses/solde du mois en cours + 5 dernieres transactions
- [x] **Rapport mensuel** - Liste des transactions avec filtres par categorie, couleurs rouge/vert
- [x] **Theme Dark Mode** - Design moderne bleu-gris et orange
- [x] **Categories en cartes** - Layout 2 colonnes avec pagination
- [x] **Authentification JWT** - Login, refresh token proactif (timer), logout
- [x] **Isolation par utilisateur** - Transactions liees au user via UserId, categories globales
- [x] **Paiement echelonne** - Depenses fixes echelonnees avec creation automatique de TransactionVariable mensuelles via le scheduler
- [x] **Refresh proactif** - Timer cote serveur qui refresh le token 3 min avant expiration
- [x] **Gestion 401** - Logout automatique et redirection /login sur 401

**Points d'attention detectes** :
- `Application.csproj` contient encore `<RootNamespace>Datas</RootNamespace>` (inconsistant avec le namespace reel)
- Interface `ITranscationService` avec faute de frappe (devrait etre `ITransactionService`)
- Appels HTTP internes (Frontend -> API sur meme serveur) - overhead inutile en Blazor Server
- Pas de projet de tests unitaires/integration

## Liens internes
- [[Architecture]] - Details architecture
- [[Entites]] - Schema complet des entites
- [[Log-Decisions]] - Historique des decisions

## Metadonnees
- **Cree le** : Decembre 2024 (base sur commits)
- **Framework** : .NET 10.0
- **Documente le** : 2025-02-03
