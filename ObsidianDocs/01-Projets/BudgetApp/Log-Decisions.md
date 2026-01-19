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

## Decisions techniques a documenter

### [A venir] - Tests unitaires
- **Contexte** : Pas de tests actuellement
- **Options a evaluer** : xUnit vs NUnit, Moq vs NSubstitute
- **Decision** : [A prendre]

### [A venir] - Authentification
- **Contexte** : Application actuellement sans auth
- **Options a evaluer** : Cookie auth vs JWT vs Identity
- **Decision** : [A prendre]

---

## Prochaines decisions a prendre

1. [ ] Ajouter projet de tests - quel framework ?
2. [ ] Gestion multi-utilisateurs - necessaire ?
3. [ ] Export donnees (CSV, PDF) - quelle lib ?
4. [ ] Graphiques/Dashboard - Chart.js ou autre ?

---

## References
- [[README]] - Vue d'ensemble du projet
- [[Architecture]] - Details architecture
- [[Entites]] - Modele de donnees
