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
