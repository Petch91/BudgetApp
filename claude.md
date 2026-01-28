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
  - `RapportService` : Generation des rapports mensuels
  - `UserService` : Gestion des utilisateurs
  - `AuthService` : Authentification (login, refresh token)

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
  - `Home.razor` : Tableau de bord (rapport mensuel)
  - `DepenseFixePage.razor` : Gestion des depenses fixes
  - `CategoriesPage.razor` : Gestion des categories
  - `Login.razor` : Page de connexion (layout: LoginLayout)
  - `Error.razor` : Page d'erreur (layout: LoginLayout)

- `Components/Layout/` :
  - `MainLayout.razor` : Layout principal avec `AuthorizeView` (protege)
  - `LoginLayout.razor` : Layout public (login, error)
  - `NavMenu.razor` : Navigation sidebar
  - `RedirectToLogin.razor` : Composant de redirection vers `/login`

- `Api/Endpoints/` : Minimal APIs
  - `DepenseFixesEndpoints.cs` : `/api/depensefixe`
  - `TransactionVariableEndpoints.cs` : `/api/transaction`
  - `CategorieEndpoints.cs` : `/api/categorie`
  - `RapportEndpoints.cs` : `/api/rapport`
  - `AuthEndpoints.cs` : `/api/auth` (login, refresh)

- `Services/` :
  - `DepenseFixeFrontService.cs` : Client HTTP depenses
  - `CategorieFrontService.cs` : Client HTTP categories
  - `TransactionFrontService.cs` : Client HTTP transactions
  - `RapportFrontService.cs` : Client HTTP rapports
  - `Notifications/AppToastService.cs` : Notifications toast

- `Services/Securite/` :
  - `AuthStateService.cs` : Gestion session (ProtectedLocalStorage)
  - `CustomAuthStateProvider.cs` : AuthenticationStateProvider Blazor
  - `Handlers/JwtAuthorizationHandler.cs` : DelegatingHandler JWT (desactive)

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
| `Users` | Utilisateurs de l'application |
| `RefreshTokens` | Tokens de rafraichissement JWT |

### Relations

```
Transaction (Base TPH)
    ├── DepenseFixe
    │       ├── DepenseDueDate (1:N)
    │       └── Rappel (1:N)
    └── TransactionVariable

Categorie (1:N) ←── Transaction

User (1:N) ←── RefreshToken
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
| Authentification | JWT Bearer | - |

---

## Authentification & Securite

### Architecture

L'authentification utilise **JWT Bearer** cote API et **AuthorizeView Blazor** cote frontend.

### Flux d'authentification

```
1. Login (/login)
   └── POST /api/auth/login → JWT AccessToken + RefreshToken
       └── AuthStateService.SaveSessionAsync() → ProtectedLocalStorage
           └── CustomAuthStateProvider.NotifyAuthStateChanged()
               └── AuthorizeView re-evalue → Authorized → MainLayout affiche

2. Navigation (page protegee)
   └── MainLayout → AuthorizeView
       ├── Authorized → affiche le contenu + NavMenu + bouton deconnexion
       └── NotAuthorized → RedirectToLogin → NavigateTo("/login")

3. Appel API (FrontService)
   └── GetClientAsync() → AuthStateService.GetAccessTokenAsync()
       └── HttpClient avec Authorization: Bearer {token}

4. Logout
   └── AuthStateService.LogoutAsync() → supprime ProtectedLocalStorage
       └── NavigateTo("/login", forceLoad: true)
```

### Composants cles

| Composant | Role |
|-----------|------|
| `AuthStateService` | Gestion session via ProtectedLocalStorage + cache memoire (`_currentSession`) |
| `CustomAuthStateProvider` | Fournit l'etat d'auth a Blazor via `AuthenticationStateProvider` |
| `AuthorizeView` (MainLayout) | Protege toutes les pages utilisant MainLayout |
| `RedirectToLogin` | Redirige vers `/login` quand non authentifie |
| `LoginLayout` | Layout public pour login et error (pas d'AuthorizeView) |

### Layouts et protection des pages

| Layout | Protection | Pages |
|--------|-----------|-------|
| `MainLayout` | `AuthorizeView` (authentification requise) | Home, DepenseFixe, Categories |
| `LoginLayout` | Aucune (public) | Login, Error |

### Token JWT dans les FrontServices

Les FrontServices utilisent une methode `GetClientAsync()` pour creer un `HttpClient` avec le token JWT :

```csharp
private async Task<HttpClient> GetClientAsync()
{
    var client = factory.CreateClient("Api");
    var token = await authState.GetAccessTokenAsync();
    if (!string.IsNullOrEmpty(token))
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    return client;
}
```

> **Note:** Le `JwtAuthorizationHandler` (DelegatingHandler) est desactive car incompatible avec ProtectedLocalStorage (JS interop non disponible dans le contexte du handler HTTP). Le token est ajoute directement dans chaque FrontService via `GetClientAsync()`.

### Configuration Blazor

- **App.razor** : `InteractiveServerRenderMode(prerender: false)` pour eviter les erreurs JS interop
- **Routes.razor** : `CascadingAuthenticationState` + `RouteView` (pas AuthorizeRouteView)
- **Program.cs** : `AddCascadingAuthenticationState()` + `AuthenticationStateProvider` enregistre comme `CustomAuthStateProvider`

### Pages et chargement des donnees

Les pages protegees chargent leurs donnees dans `OnAfterRenderAsync(firstRender)` pour garantir que JS interop est disponible :

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender && !_initialized)
    {
        _initialized = true;
        await AuthState.GetSessionAsync(); // Pre-charge la session
        await ChargerDonnees();
        StateHasChanged();
    }
}
```

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
         ▼ FrontService.GetClientAsync() + JWT Token
API Endpoints (Minimal APIs) [Authorize]
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

---

## Deploiement

### Docker

**Dockerfile** a la racine du projet - build multi-stage :

```bash
# Build
docker build -t budgetapp .

# Run
docker run -p 5201:5201 budgetapp
```

**Configuration Docker :**
- Image build : `mcr.microsoft.com/dotnet/sdk:10.0`
- Image runtime : `mcr.microsoft.com/dotnet/aspnet:10.0`
- Port : 5201
- Environnement : Production

**Variables d'environnement :**
| Variable | Valeur |
|----------|--------|
| `ASPNETCORE_ENVIRONMENT` | Production |
| `ASPNETCORE_URLS` | http://+:5201 |

### Reverse Proxy (Traefik)

L'app tourne en HTTP (port 5201). Traefik gere le SSL termination :

```
Client → HTTPS → Traefik → HTTP:5201 → App
```

> **Note :** `UseHttpsRedirection()` et `UseHsts()` sont desactives car Traefik gere HTTPS.

### Profils de lancement

| Profil | Environnement | Usage |
|--------|--------------|-------|
| `http` | Development | Dev local avec Swagger |
| `http Prod` | Production | Test config Production |

### Static Web Assets

- **Development** (`dotnet run`) : Assets mappes dynamiquement depuis NuGet
- **Production** (`dotnet publish` / Docker) : Assets copies physiquement dans le build
