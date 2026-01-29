# Log des Decisions - BudgetApp

## Format

Chaque decision est documentee ainsi :
- **Date** : Quand
- **Contexte** : Pourquoi cette question s'est posee
- **Options envisagees** : Alternatives
- **Decision** : Ce qui a ete choisi
- **Raison** : Pourquoi ce choix

---

## Decisions architecturales initiales

### 2024-12 - Architecture initiale

**Contexte** : Demarrage du projet de gestion de budget personnel

**Decisions identifiees** :

#### 1. Choix Blazor Server
- **Options** : Blazor Server vs Blazor WebAssembly vs Hybrid
- **Decision** : Blazor Server
- **Raison probable** :
  - Acces direct a la base de donnees
  - Pas besoin de fonctionnement offline
  - Simplicite de deploiement
  - Meilleure performance initiale

#### 2. Structure en 4 projets
- **Options** : Monolithique vs Multi-projets
- **Decision** : 4 projets (Entities, Application, Shared, Frontend)
- **Raison** :
  - Separation claire des responsabilites
  - Reutilisabilite des composants (BudgetApp.Shared)
  - Facilite les tests (isolation des couches)
  - Pattern Clean Architecture

#### 3. Entity Framework Core avec TPH
- **Options** : TPH vs TPT vs Table separees
- **Decision** : TPH (Table Per Hierarchy)
- **Raison** :
  - Transaction est la classe de base commune
  - Requetes plus simples (une seule table)
  - Bonnes performances pour ce volume de donnees

#### 4. FluentResults pour gestion d'erreurs
- **Options** : Exceptions vs Result Pattern
- **Decision** : FluentResults
- **Raison** :
  - Explicite dans les signatures de methodes
  - Pas de gestion d'exceptions pour les cas metier
  - Meilleure lisibilite du code

#### 5. Minimal APIs vs Controllers
- **Options** : Controllers MVC vs Minimal APIs
- **Decision** : Minimal APIs
- **Raison** :
  - Plus leger et moderne (.NET 6+)
  - Moins de boilerplate
  - Suffisant pour une API simple

#### 6. Blazor.Bootstrap pour l'UI
- **Options** : MudBlazor vs Blazor.Bootstrap vs Radzen vs custom
- **Decision** : Blazor.Bootstrap 3.5.0
- **Raison** :
  - Composants Bootstrap natifs
  - Bonne documentation
  - Composants complets (Grid, Modal, Tabs, etc.)

---

### 2025-01-12 - Refactoring namespace

**Contexte** : Inconsistance entre le nom du projet "Application" et le namespace "Datas"

**Decision** : Renommer namespace `Datas` -> `Application`

**Raison** :
- Coherence nom projet / namespace
- Meilleure lisibilite du code
- Convention standard .NET

**Fichiers modifies** : 14 fichiers dans Application + Frontend

---

### 2025-01-12 - Ajout composant TransactionVariable

**Contexte** : Besoin de gerer les transactions ponctuelles (revenus/depenses)

**Decisions** :

1. **Page unifiee avec onglets**
   - Options : Pages separees vs Onglets
   - Decision : Une page `/depenses` avec 2 onglets
   - Raison : Navigation simplifiee, vue d'ensemble

2. **Composant TransactionVariable_C**
   - Design similaire a DepenseFixe_C pour coherence
   - Navigation par mois (pas de rappels)
   - Statistiques : Revenus, Depenses, Solde

3. **Service TransactionFrontService**
   - Nouvelle interface IHttpTransaction
   - Combine revenus + depenses par mois

---

### 2025-01-16 - Theme Dark Mode et améliorations UI

**Contexte** : L'interface utilisateur nécessitait des améliorations visuelles pour le mode sombre

**Decisions** :

1. **Organisation des styles CSS**
   - Styles globaux (theme, Bootstrap overrides) → `wwwroot/app.css`
   - Styles specifiques aux composants → `Composant.razor.css` (CSS isolation)
   - Pas de balises `<style>` inline dans les fichiers `.razor`
   - Raison : Separation des responsabilites, encapsulation, maintenance simplifiee

2. **Theme Dark Mode personnalisé**
   - Variables CSS dans `app.css` : `--bg-primary`, `--bg-secondary`, `--accent-primary`, etc.
   - Couleurs : Bleu-gris (#1a1d29) + Orange (#ff8c42)
   - Override des classes Bootstrap pour le dark mode

2. **Couleurs semantiques pour les montants**
   - Revenus en vert vif (#22c55e)
   - Depenses en rouge vif (#ef4444)
   - Applique dans le Rapport et le Tableau de bord

3. **Onglets (Tabs) ameliores**
   - Style `.nav-underline` pour Blazor Bootstrap
   - Onglet actif en orange, inactifs en blanc
   - Hover avec effet bleu subtil

4. **Correction hauteur des lignes de table**
   - Padding reduit de `1rem` a `0.5rem 0.75rem`
   - Meilleure densite d'information

---

### 2025-01-16 - Tableau de bord (Home.razor)

**Contexte** : La page d'accueil etait vide ("Hello World")

**Decision** : Creer un dashboard avec les totaux du mois en cours

**Implementation** :
- 3 cartes : Revenus, Depenses, Solde du mois
- Couleurs dynamiques selon le solde (vert positif, rouge negatif)
- Liste des 5 dernieres transactions
- Injection de `IHttpRapport` pour recuperer les donnees

---

### 2025-01-16 - Service Rapport

**Contexte** : Besoin d'un endpoint pour obtenir le rapport mensuel

**Decisions** :
- Nouveau DTO `RapportMoisDto` (TotalRevenus, TotalDepenses, Solde, Lignes)
- Nouveau DTO `RapportLigneDto` (Date, Intitule, Montant, Categorie, IsRevenu, IsDepenseFixe)
- Service `RapportService` dans Application
- Service HTTP `RapportFrontService` dans Frontend
- Endpoint `/api/rapport/{annee}/{mois}`

---

### 2025-01-16 - Refonte composant Categories

**Contexte** : Le composant Categories utilisait une grille (Grid) qui n'affichait qu'une colonne, limitant la visibilite

**Decisions** :

1. **Layout en cartes sur 2 colonnes**
   - Options : Grid vs Cards
   - Decision : Cartes Bootstrap avec `col-md-6`
   - Raison : Meilleure utilisation de l'espace, plus visuel

2. **Selection par clic**
   - Remplacement de la selection Grid par clic sur carte
   - Effet visuel : bordure orange + fond transparent orange
   - Icone check pour la carte selectionnee

3. **Pagination integree**
   - 10 categories par page
   - Navigation < 1 2 3 > stylee dark mode
   - Affichee uniquement si > 10 categories

4. **Styles CSS dedies**
   - `.category-card` avec hover (bordure bleue, elevation)
   - `.category-card.selected` (bordure orange, fond orange transparent)
   - `.category-icon` (fond eleve, icone bleue)

**Fichiers modifies** :
- `Categories_C.razor` - Nouveau template avec cartes
- `Categories_C.razor.cs` - Logique pagination et selection
- `app.css` - Styles category-card et pagination

---

### 2025-01 - Authentification JWT + Blazor AuthorizeView

**Contexte** : Application sans authentification, besoin de proteger l'acces

**Decisions** :

#### 1. JWT Bearer pour l'API
- **Options** : Cookie auth vs JWT vs ASP.NET Identity
- **Decision** : JWT Bearer
- **Raison** :
  - Stateless, adapte a une API REST
  - AccessToken + RefreshToken
  - Separation frontend/API claire

#### 2. ProtectedLocalStorage pour la session frontend
- **Options** : Cookies vs LocalStorage vs ProtectedLocalStorage
- **Decision** : ProtectedLocalStorage
- **Raison** :
  - Chiffrement automatique cote serveur
  - Integration native Blazor Server
  - Pas de transmission automatique au serveur (contrairement aux cookies)

#### 3. AuthorizeView dans MainLayout (pas AuthorizeRouteView)
- **Options** : `[Authorize]` sur les pages vs `AuthorizeRouteView` dans Routes.razor vs `AuthorizeView` dans MainLayout
- **Decision** : `AuthorizeView` dans MainLayout
- **Raison** :
  - Protege TOUTES les pages utilisant MainLayout sans attribut individuel
  - Pages publiques utilisent LoginLayout (pas d'auth)
  - `AuthorizeRouteView` dans Routes.razor causait des boucles de redirection infinies quand combine avec MainLayout

#### 4. Token JWT ajoute dans les FrontServices (pas DelegatingHandler)
- **Options** : `JwtAuthorizationHandler` (DelegatingHandler) vs Token dans chaque FrontService
- **Decision** : Token dans les FrontServices via `GetClientAsync()`
- **Raison** :
  - Le DelegatingHandler s'execute dans un contexte HTTP different du circuit Blazor
  - ProtectedLocalStorage (JS interop) n'est pas accessible dans le handler
  - Erreur `InvalidOperationException: JavaScript interop calls cannot be issued at this time`
  - Le pattern `GetClientAsync()` fonctionne car il est appele dans le contexte du composant Blazor

#### 5. Prerendering desactive
- **Options** : Prerender true vs false
- **Decision** : `InteractiveServerRenderMode(prerender: false)` dans App.razor
- **Raison** :
  - ProtectedLocalStorage necessite JS interop
  - JS interop non disponible pendant le rendu statique
  - Desactiver le prerendering evite toutes les erreurs JS interop

#### 6. Chargement des donnees dans OnAfterRenderAsync
- **Options** : `OnInitializedAsync` vs `OnAfterRenderAsync`
- **Decision** : `OnAfterRenderAsync(firstRender)` pour les pages protegees
- **Raison** :
  - Garantit que JS interop est disponible
  - Permet de pre-charger la session avant les appels HTTP
  - Evite les erreurs de rendu statique

**Fichiers crees/modifies** :
- `Services/Securite/AuthStateService.cs` - Gestion session
- `Services/Securite/CustomAuthStateProvider.cs` - AuthenticationStateProvider
- `Services/Securite/Handlers/JwtAuthorizationHandler.cs` - Desactive
- `Components/Layout/MainLayout.razor` - AuthorizeView + bouton deconnexion
- `Components/Layout/LoginLayout.razor` - Layout public
- `Components/Layout/RedirectToLogin.razor` - Redirection
- `Components/Pages/Login.razor` - Page de connexion
- `Components/Routes.razor` - CascadingAuthenticationState + RouteView
- `App.razor` - prerender: false

---

### 2025-01 - Deploiement Docker et suppression HTTPS

**Contexte** : Deploiement de l'app derriere Traefik (reverse proxy) qui gere le SSL

**Decisions** :

#### 1. Suppression de UseHttpsRedirection et UseHsts
- **Options** : Garder HTTPS dans l'app vs deleguer a Traefik
- **Decision** : Supprimer `UseHttpsRedirection()` et `UseHsts()`
- **Raison** :
  - Traefik gere le SSL termination
  - L'app recoit du HTTP de Traefik, pas du HTTPS du client
  - Ces middlewares causaient des 404 sur les fichiers statiques en Production

#### 2. Port 5201 fixe
- **Decision** : Forcer le port 5201 via `ASPNETCORE_URLS=http://+:5201`
- **Raison** : Coherence entre environnements, configuration Traefik simplifiee

#### 3. Dockerfile multi-stage
- **Decision** : SDK pour build, aspnet pour runtime
- **Raison** : Image finale legere (runtime only)

**Fichiers crees/modifies :**
- `Dockerfile` - Nouveau fichier a la racine
- `Program.cs` - Suppression UseHttpsRedirection/UseHsts

---

### 2025-01 - Isolation des transactions par utilisateur (UserId)

**Contexte** : Les transactions etaient accessibles par tous les utilisateurs authentifies, sans isolation des donnees

**Decisions** :

#### 1. UserId sur Transaction (classe de base TPH)
- **Options** : UserId sur chaque sous-classe vs sur la classe de base
- **Decision** : UserId sur `Transaction` (classe de base)
- **Raison** :
  - Heritage TPH : la colonne est dans la table unique `Transactions`
  - DepenseFixe et TransactionVariable heritent automatiquement
  - Une seule FK, une seule relation a configurer

#### 2. Categories globales (pas de filtre par userId)
- **Options** : Categories par utilisateur vs Categories partagees
- **Decision** : Categories globales
- **Raison** :
  - Simplifie la gestion (pas de doublons)
  - Les categories sont generiques (Alimentation, Transport, etc.)

#### 3. Extraction userId via ClaimTypes.NameIdentifier (pas JwtRegisteredClaimNames.Sub)
- **Options** : `JwtRegisteredClaimNames.Sub` vs `ClaimTypes.NameIdentifier`
- **Decision** : `ClaimTypes.NameIdentifier`
- **Raison** :
  - ASP.NET Core remappe automatiquement le claim JWT `"sub"` vers `ClaimTypes.NameIdentifier`
  - `FindFirst(JwtRegisteredClaimNames.Sub)` retourne `null` a cause de ce remapping
  - Bug decouvert en runtime (NullReferenceException)

#### 4. Default value UserId = 1 pour la migration
- **Decision** : `HasDefaultValue(1)` sur la colonne `UserId`
- **Raison** : Les donnees existantes sont attribuees au premier utilisateur sans perte

**Fichiers modifies** : 18 fichiers (modeles, interfaces, services, endpoints, migration)

---

## Decisions techniques a documenter

### [A venir] - Tests unitaires
- **Contexte** : Pas de tests actuellement
- **Options a evaluer** : xUnit vs NUnit, Moq vs NSubstitute
- **Decision** : [A prendre]

---

## Prochaines decisions a prendre

1. [ ] Ajouter projet de tests - quel framework ?
2. [ ] Export donnees (CSV, PDF) - quelle lib ?
3. [ ] Graphiques/Dashboard - Chart.js ou autre ?

---

## References
- [[README]] - Vue d'ensemble du projet
- [[Architecture]] - Details architecture
- [[Entites]] - Modele de donnees
