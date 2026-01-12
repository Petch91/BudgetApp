# BudgetApp - Architecture Documentation

## Vue d'ensemble

BudgetApp est une application .NET 10 de gestion de budget suivant les principes de **Clean Architecture** avec une approche Service-Oriented.

## Structure du projet

```
BudgetApp/
├── Entities/                          # Entites du domaine & contrats
├── Application/                       # Logique metier & persistance
├── BudgetApp.Shared/                  # Composants Blazor partages
├── Front_BudgetApp/
│   ├── Front_BudgetApp/               # Application Blazor Server principale
│   └── Front_BudgetApp.Client/        # Composants Blazor cote client
└── BudgetApp.sln
```

## Couches de l'architecture

### 1. Entities (Couche Domaine)

**Responsabilites:**
- Modeles du domaine
- DTOs pour les contrats API
- Formulaires d'entree avec validation
- Interfaces du domaine

**Structure:**
- `Domain/Models/` : Entites principales
  - `Transaction` (classe de base - heritage)
  - `DepenseFixe` (depenses fixes recurrentes)
  - `TransactionVariable` (transactions variables)
  - `Categorie` (categories de depenses)
  - `DepenseMois` (resumes mensuels)
  - `DepenseDueDate` (echeances)
  - `Rappel` (rappels/alertes)

- `Contracts/Dtos/` : Data Transfer Objects
- `Contracts/Forms/` : Formulaires de saisie
- `Domain/Interfaces/` : `IModel`, `ITransaction`
- `Domain/Mappers/` : Mapping Form -> Entity

**Dependances:** FluentValidation, FluentResults, Serilog

---

### 2. Application (Couche Services & Persistance)

**Namespace:** `Application`

**Responsabilites:**
- Logique metier via les classes de service
- Acces aux donnees via Entity Framework Core
- DbContext et migrations
- Mapping entites <-> DTOs

**Structure:**

- `Persistence/MyDbContext.cs` : EF Core DbContext
  - Strategie d'heritage **TPH** (Table Per Hierarchy)
  - Discriminateur: "TransactionTable"
  - Triggers SQL configures
  - Seeding "NoCategory" par defaut

- `Services/` :
  - `DepenseFixeService` : Operations CRUD + generation echeances
  - `TransactionService` : Transactions variables
  - `CategorieService` : Gestion des categories

- `Interfaces/` :
  - `IRepository<TDto>` : Interface generique lecture/ecriture
  - `IDepenseFixeService`, `ITranscationService`, `ICategorieService`

- `Projections/ProjectionDto.cs` : Expressions LINQ pour projection efficace
- `Mappers/MapperDto.cs` : Extensions de mapping

**Dependances:** EF Core 10.0.1 (SqlServer), FluentValidation, FluentResults, Serilog

---

### 3. BudgetApp.Shared (Composants partages)

**Type:** Razor Class Library (.NET 10)

**Responsabilites:**
- Composants Blazor reutilisables
- Interfaces de services partages
- Utilitaires

**Structure:**

- `Components/Transactions/` :
  - `DepenseFixe_C.razor` : Grille des depenses fixes

- `Components/Categories/` :
  - `Categories_C.razor` : Gestion des categories
  - `CategorieForm_C.razor` : Formulaire categorie

- `Interfaces/Http/` :
  - `IHttpDepenseFixe` : Service HTTP depenses fixes
  - `IHttpCategorie` : Service HTTP categories

- `Tools/` :
  - `SerilogConfig.cs` : Configuration logging
  - `Icones.cs` : Definitions d'icones

**Dependances:** Blazor.Bootstrap 3.5.0, Blazored.FluentValidation, Serilog

---

### 4. Front_BudgetApp (Application principale)

**Type:** Blazor Server (.NET 10)

**Responsabilites:**
- Application web principale
- Endpoints API REST
- Integration des composants partages
- Services frontend
- Taches planifiees

**Structure:**

- `Program.cs` : Configuration DI et demarrage
  - Render mode: InteractiveServer
  - Registration des services
  - Configuration HttpClient
  - Background service (DepenseFixeScheduler)

- `Components/Pages/` :
  - `DepenseFixePage.razor`
  - `CategoriesPage.razor`
  - `Home.razor`

- `Api/Endpoints/` : Minimal APIs
  - `DepenseFixesEndpoints.cs` : `/api/depensefixe`
  - `TransactionVariableEndpoints.cs` : `/api/transaction`
  - `CategorieEndpoints.cs` : `/api/categorie`

- `Services/` :
  - `DepenseFixeFrontService.cs` : Client HTTP depenses
  - `CategorieFrontService.cs` : Client HTTP categories
  - `Notifications/AppToastService.cs` : Notifications toast

- `Tools/DepenseFixeScheduler.cs` : Service de maintenance planifie

---

## Base de donnees

**Technologie:** SQL Server avec EF Core Code-First

**Strategie d'heritage:** TPH (Table Per Hierarchy)

### Tables principales

| Table | Description |
|-------|-------------|
| `Transactions` | Table de base avec discriminateur TPH |
| `Categories` | Categories de depenses |
| `DepenseDueDates` | Echeances des depenses fixes |
| `Rappels` | Rappels/alertes |
| `DepenseMois` | Resumes mensuels |

### Relations

```
Transaction (Base TPH)
    ├── DepenseFixe
    │       ├── DepenseDueDate (1:N)
    │       └── Rappel (1:N)
    └── TransactionVariable

Categorie (1:N) ←── Transaction
```

---

## Technologies utilisees

| Composant | Technologie | Version |
|-----------|-------------|---------|
| Runtime | .NET | 10.0 |
| Framework Web | ASP.NET Core Blazor | 10.0.1 |
| Composants UI | Blazor.Bootstrap | 3.5.0 |
| ORM | Entity Framework Core | 10.0.1 |
| Base de donnees | SQL Server | - |
| Validation | FluentValidation | 12.1.1 |
| Gestion erreurs | FluentResults | 4.0.0 |
| Logging | Serilog | 4.3.1 |
| Documentation API | Swagger/Swashbuckle | 10.0.1 |

---

## Taches planifiees

### DepenseFixeScheduler

**Frequence:** Toutes les heures

**Operations:**
1. **Nettoyage:** Suppression des echeances et rappels expires (> 4 jours)
2. **Generation:** Creation des prochaines echeances si necessaire (horizon 2 mois)

**Calcul frequence:**
- Mensuel: +1 mois (12x/an)
- Trimestriel: +3 mois (4x/an)
- Biannuel: +6 mois (2x/an)
- Annuel: +1 an (1x/an)

---

## Patterns et pratiques

### Validation
- **FluentValidation** pour les regles de validation
- Integration Blazor via **Blazored.FluentValidation**

### Gestion des erreurs
- Pattern **FluentResults** : `Result<T>` pour retours typés
- Pas d'exceptions pour les erreurs attendues

### Logging
- **Serilog** avec sinks Console, Debug, File
- Logs rotatifs journaliers dans `C:\logs\{ServiceName}`
- Retention: 31 jours

### API Design
- **ASP.NET Core Minimal APIs**
- Groupes d'endpoints avec tags Swagger
- Codes HTTP: 200, 201, 204, 400, 404

---

## Flux de donnees

```
Frontend (Blazor Components)
         │
         ▼ HTTP Client
API Endpoints (Minimal APIs)
         │
         ▼
Application Services
         │
         ▼
EF Core DbContext
         │
         ▼
SQL Server Database
```

---

## Configuration

### Connection String
- Serveur: `192.168.50.48`
- Base: `BudgetDB`
- Authentification: SQL Server (budget_user)

### Environnement
- Swagger disponible en Development
- Logs niveau Information (Warning pour framework)
