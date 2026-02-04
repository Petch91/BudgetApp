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
├── BudgetApp.Mobile/                  # App mobile MAUI Blazor Hybrid (iOS/Android)
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
  - `DepenseFixe` (depenses fixes recurrentes, support echelonnement)
  - `TransactionVariable` (transactions variables)
  - `Categorie` (categories de depenses)
  - `DepenseMois` (resumes mensuels)
  - `DepenseDueDate` (echeances)
  - `Rappel` (rappels/alertes)
  - `User` (utilisateurs)
  - `RefreshToken` (tokens de rafraichissement JWT)

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
  - `DepenseFixeService` : Operations CRUD + generation echeances (filtre par userId)
  - `TransactionService` : Transactions variables (filtre par userId)
  - `CategorieService` : Gestion des categories (globales, pas de filtre userId)
  - `RapportService` : Generation des rapports mensuels (filtre par userId)
  - `UserService` : Gestion des utilisateurs
  - `AuthService` : Authentification (login, refresh token, logout)

- `Services/Securite/` :
  - `PasswordManager` : Hash et verification des mots de passe (IPasswordHasher)
  - `JwtTokenGenerator` : Generation des tokens JWT (IJwtTokenGenerator)

- `Interfaces/` :
  - `IRepository<TDto>` : Interface generique lecture/ecriture (toutes les methodes prennent un `userId`)
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
  - `TransactionVariable_C.razor` : Gestion des transactions variables

- `Components/Categories/` :
  - `Categories_C.razor` : Gestion des categories
  - `CategorieForm_C.razor` : Formulaire categorie

- `Components/Rapport/` :
  - `Rapport_C.razor` : Rapport mensuel avec filtres

- `Interfaces/Http/` :
  - `IHttpDepenseFixe` : Service HTTP depenses fixes
  - `IHttpCategorie` : Service HTTP categories
  - `IHttpTransaction` : Service HTTP transactions variables
  - `IHttpRapport` : Service HTTP rapports

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
  - `AuthEndpoints.cs` : `/api/auth` (login, refresh, logout)

- `Services/` :
  - `DepenseFixeFrontService.cs` : Client HTTP depenses
  - `CategorieFrontService.cs` : Client HTTP categories
  - `TransactionFrontService.cs` : Client HTTP transactions
  - `RapportFrontService.cs` : Client HTTP rapports
  - `HttpErrorHelper.cs` : Messages d'erreur utilisateur
  - `Notifications/AppToastService.cs` : Notifications toast

- `Services/Securite/` :
  - `AuthStateService.cs` : Gestion session (ProtectedLocalStorage) + timer refresh proactif
  - `CustomAuthStateProvider.cs` : AuthenticationStateProvider Blazor
  - `Handlers/JwtAuthorizationHandler.cs` : DelegatingHandler JWT (desactive)

- `Tools/DepenseFixeScheduler.cs` : Service de maintenance planifie

---

### 5. BudgetApp.Mobile (Application mobile)

**Type:** .NET MAUI Blazor Hybrid (.NET 10)

**Responsabilites:**
- Application mobile iOS/Android
- Mode offline avec SQLite
- Synchronisation bidirectionnelle
- Authentification biometrique (Face ID / Touch ID)

**Structure:**

```
BudgetApp.Mobile/
├── Components/
│   ├── Pages/
│   │   ├── HomePage.razor         # Rapport mensuel (revenus/depenses/solde)
│   │   ├── TransactionsPage.razor # Liste + ajout/edit transactions (bottom sheet)
│   │   └── LoginPage.razor        # Auth email/password + biometrie
│   └── Layout/
│       └── MobileLayout.razor     # Navigation bottom tabs + indicateur offline
│
├── Services/
│   ├── Api/                       # Clients HTTP (meme pattern que FrontServices)
│   │   ├── MobileTransactionService.cs
│   │   ├── MobileRapportService.cs
│   │   └── MobileCategorieService.cs
│   │
│   ├── Auth/
│   │   ├── MobileAuthStateService.cs  # SecureStorage (pas ProtectedLocalStorage)
│   │   └── BiometricService.cs        # Face ID / Touch ID via Plugin.Fingerprint
│   │
│   ├── Offline/
│   │   ├── LocalDbContext.cs          # SQLite EF Core
│   │   ├── SyncService.cs             # Sync bidirectionnel (Last-Write-Wins)
│   │   └── ConnectivityService.cs     # Detection online/offline
│   │
│   └── Hybrid/                    # Orchestration online/offline
│       ├── TransactionHybridService.cs
│       └── RapportHybridService.cs
│
├── Models/Local/
│   ├── LocalTransaction.cs        # Entite SQLite avec SyncState
│   ├── LocalCategorie.cs          # Cache categories (read-only)
│   └── SyncMetadata.cs            # Tracking sync
│
├── Platforms/
│   ├── Android/                   # AndroidManifest.xml, MainActivity.cs
│   └── iOS/                       # Info.plist (Face ID), AppDelegate.cs
│
└── wwwroot/
    └── css/mobile.css             # Styles mobile (safe areas, dark mode)
```

**Architecture offline-first:**

```
App Start
    │
    ▼
Charger depuis SQLite (immediat)
    │
    ▼
Si online → Sync en background
    │
    ├── Pull: GET /api/rapport/{year}/{month}
    │         → Update LocalTransaction
    │
    └── Push: Envoyer PendingCreate/Update/Delete
              → Marquer comme Synced
```

**Modele de sync (SyncState):**
- `Synced` (0) : Synchronise avec le serveur
- `PendingCreate` (1) : Cree offline, pas encore envoye
- `PendingUpdate` (2) : Modifie offline
- `PendingDelete` (3) : Supprime offline

**Resolution de conflits:** Last-Write-Wins base sur `LastModified`

**Dependances specifiques:**
- `Microsoft.EntityFrameworkCore.Sqlite` : Base locale
- `Plugin.Fingerprint` : Biometrie
- `System.IdentityModel.Tokens.Jwt` : Parsing JWT

**Configuration API (MauiProgram.cs):**
- Android emulateur: `http://10.0.2.2:5201`
- iOS simulateur: `http://localhost:5201`
- Production: A configurer

---

## Base de donnees

**Technologie:** SQL Server avec EF Core Code-First

**Strategie d'heritage:** TPH (Table Per Hierarchy)

### Tables principales

| Table | Description |
|-------|-------------|
| `Transactions` | Table de base avec discriminateur TPH (contient `UserId` FK vers Users) |
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

User (1:N) ←── Transaction
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
| Authentification | JWT Bearer | 10.0.2 |

---

## Authentification & Securite

### Architecture

L'authentification utilise **JWT Bearer** cote API et **AuthorizeView Blazor** cote frontend.

### Flux d'authentification

```
1. Login (/login)
   └── POST /api/auth/login → JWT AccessToken + RefreshToken
       └── AuthStateService.SaveSessionAsync() → ProtectedLocalStorage
           └── ScheduleRefresh() → Timer proactif
               └── CustomAuthStateProvider.NotifyAuthStateChanged()
                   └── AuthorizeView re-evalue → Authorized → MainLayout affiche

2. Navigation (page protegee)
   └── MainLayout → AuthorizeView
       ├── Authorized → affiche le contenu + NavMenu + bouton deconnexion
       └── NotAuthorized → RedirectToLogin → NavigateTo("/login")

3. Appel API (FrontService)
   └── GetClientAsync() → AuthStateService.GetAccessTokenAsync()
       └── HttpClient avec Authorization: Bearer {token}
       └── Si 401 → ForceLogoutAsync() → OnSessionExpired → redirect /login

4. Refresh proactif (timer)
   └── AuthStateService.ScheduleRefresh()
       └── Timer se declenche 3 min avant expiration
           └── RefreshSessionAsync() → nouveau token
               └── ScheduleRefresh() → nouveau timer
       └── Si refresh echoue → ForceLogoutAsync() → redirect /login

5. Logout
   └── AuthStateService.LogoutAsync() → CancelRefreshTimer() + supprime ProtectedLocalStorage
       └── NavigateTo("/login", forceLoad: true)
```

### Composants cles

| Composant | Role |
|-----------|------|
| `AuthStateService` | Gestion session via ProtectedLocalStorage + cache memoire + timer refresh proactif |
| `CustomAuthStateProvider` | Fournit l'etat d'auth a Blazor via `AuthenticationStateProvider` |
| `AuthorizeView` (MainLayout) | Protege toutes les pages utilisant MainLayout |
| `RedirectToLogin` | Redirige vers `/login` quand non authentifie |
| `LoginLayout` | Layout public pour login et error (pas d'AuthorizeView) |

### Layouts et protection des pages

| Layout | Protection | Pages |
|--------|-----------|-------|
| `MainLayout` | `AuthorizeView` (authentification requise) | Home, DepenseFixe, Categories |
| `LoginLayout` | Aucune (public) | Login, Error |

### Refresh Token Proactif

L'`AuthStateService` implemente un timer qui rafraichit le token **avant** son expiration :

```csharp
// Timer declenche a ExpiresAt - 3 minutes
private void ScheduleRefresh()
{
    var delay = _currentSession.ExpiresAt - DateTime.UtcNow - RefreshMargin;
    _ = Task.Run(async () => {
        await Task.Delay(delay, token);
        await RefreshSessionAsync();
    });
}
```

**Evenements** :
- `OnAuthStateChanged` : Notifie les changements d'etat d'auth
- `OnSessionExpired` : Declenche une redirection vers `/login` (ecoute par MainLayout)

### Gestion des 401 dans les FrontServices

Tous les FrontServices detectent les reponses 401 et declenchent un logout automatique :

```csharp
if (response.StatusCode == HttpStatusCode.Unauthorized)
{
    await authState.ForceLogoutAsync();
    return Result.Fail("Session expiree");
}
```

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

### Configuration JWT

| Parametre | Development | Production |
|-----------|-------------|------------|
| `ExpirationMinutes` | 30 | 30 |
| RefreshToken | 7 jours | 7 jours |
| RefreshMargin | 3 minutes | 3 minutes |

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
1. **Nettoyage:** Suppression des rappels vus et expires (> 5 jours)
2. **Echelonnement:** Pour les depenses echelonnees (`IsEchelonne && EcheancesRestantes > 0`), cree une TransactionVariable mensuelle automatique. Date calculee depuis la premiere DueDate (`startDate.AddMonths(numero - 1)`). Decremente `EcheancesRestantes`, ajuste la derniere echeance.
3. **Generation:** Creation des prochaines echeances si necessaire (horizon 2 mois) - depenses non echelonnees uniquement

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
- Pattern **FluentResults** : `Result<T>` pour retours types
- Pas d'exceptions pour les erreurs attendues

### Logging
- **Serilog** avec sinks Console, Debug, File
- Logs rotatifs journaliers dans `C:\logs\{ServiceName}`
- Retention: 31 jours

### API Design
- **ASP.NET Core Minimal APIs**
- Groupes d'endpoints avec tags Swagger
- Codes HTTP: 200, 201, 204, 400, 404

### Regles de code

#### CSS
- **Jamais de balise `<style>` dans les fichiers `.razor`**
- CSS specifique a un composant → fichier `Composant.razor.css` (CSS isolation Blazor)
- CSS partage entre plusieurs composants → `wwwroot/app.css`

#### TPH et colonnes nullable
- En TPH, les proprietes specifiques a un sous-type sont nullable en SQL
- Pour filtrer un bool en LINQ, utiliser `== false` (pas `!property`) car NULL n'est ni true ni false en SQL
- Exemple : `Where(d => d.IsEchelonne == false)` et non `Where(d => !d.IsEchelonne)`

#### Classes et records
- **Chaque class/record dans son propre fichier dedie** (pas de types imbriques ou accoles dans un autre fichier)
- Ranger dans le bon dossier selon le role :
  - Modeles domaine → `Entities/Domain/Models/`
  - DTOs → `Entities/Contracts/Dtos/`
  - Formulaires/Requests → `Entities/Contracts/Forms/`
  - Interfaces → dans le projet concerne (`Domain/Interfaces/`, `Application/Interfaces/`, etc.)

---

## Flux de donnees

```
Frontend (Blazor Components)
         │
         ▼ FrontService.GetClientAsync() + JWT Token
API Endpoints (Minimal APIs) [Authorize]
         │ Extraction userId via ClaimTypes.NameIdentifier
         ▼
Application Services (filtre par userId)
         │
         ▼
EF Core DbContext
         │
         ▼
SQL Server Database
```

### Isolation des donnees par utilisateur

- Les **transactions** (DepenseFixe, TransactionVariable) sont liees a un `UserId` (FK vers Users)
- Les endpoints API extraient le `userId` du JWT via `ClaimTypes.NameIdentifier`
- Les services filtrent toutes les requetes par `userId` et l'assignent lors de la creation
- Les **categories** restent globales (partagees entre tous les utilisateurs)
- Les donnees existantes avant la migration sont attribuees au user Id=1 (default)

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

---

## Etat actuel & Prochaines etapes

### BudgetApp.Mobile - Statut

**Statut:** Code complet, non teste (workloads MAUI non installes sur Windows)

**Ce qui a ete cree:**
- Structure complete du projet MAUI Blazor Hybrid
- Pages: Login (biometrie), Home (rapport), Transactions (CRUD)
- Services: Auth (SecureStorage), Offline (SQLite), Sync, API clients
- Platforms: Android (manifest, permissions) et iOS (Info.plist, Face ID)
- Styles CSS mobile-first avec support dark mode et safe areas

**Prochaines etapes sur Mac:**

1. **Installer les prerequis:**
   ```bash
   # .NET 10 SDK
   brew install --cask dotnet-sdk

   # MAUI workloads
   dotnet workload install maui

   # Xcode depuis App Store (pour iOS)
   ```

2. **Restaurer et builder:**
   ```bash
   cd BudgetApp
   dotnet restore BudgetApp.Mobile/BudgetApp.Mobile.csproj
   dotnet build BudgetApp.Mobile -f net10.0-ios
   ```

3. **Lancer sur simulateur iOS:**
   ```bash
   dotnet build BudgetApp.Mobile -f net10.0-ios -t:Run
   ```

4. **Configurer l'API:**
   - Modifier `MauiProgram.cs` pour pointer vers l'API backend
   - S'assurer que l'API est accessible depuis le simulateur

5. **Tester:**
   - Login avec email/password
   - Activer Face ID (simulateur: Features > Face ID > Enrolled)
   - Ajouter/modifier des transactions
   - Tester le mode offline (mode avion)
   - Verifier la sync a la reconnexion

**Points d'attention potentiels:**
- Les packages NuGet peuvent avoir des versions incompatibles avec .NET 10
- `Plugin.Fingerprint` est en beta (3.0.0-beta.1)
- Tester la compatibilite EF Core SQLite sur iOS/Android
