# Donnees de test - BudgetApp

Donnees a utiliser pour les tests manuels ou automatises.

---

## Depenses Fixes

| Intitule | Montant | Categorie | Frequence | Domiciliee |
|----------|---------|-----------|-----------|------------|
| Netflix | 15.99 | Domotique | Mensuel | Non |
| Spotify | 9.99 | Domotique | Mensuel | Oui |
| Loyer | 850.00 | Logement | Mensuel | Oui |
| Electricite | 75.00 | Energie | Mensuel | Oui |
| Assurance Auto | 45.00 | Assurances | Mensuel | Oui |
| Salle de sport | 29.90 | Medical | Mensuel | Oui |
| Internet | 39.99 | Telecom | Mensuel | Oui |
| Impots fonciers | 1200.00 | Impots | Annuel | Non |

---

## Transactions Variables - Revenus

| Intitule | Montant | Categorie | Date (format) |
|----------|---------|-----------|---------------|
| Salaire | 2500.00 | Travail | 1er du mois |
| Prime | 500.00 | Travail | Date du jour |
| Remboursement Secu | 45.00 | Medical | Date du jour |
| Vente Leboncoin | 150.00 | Cadeaux | Date du jour |

---

## Transactions Variables - Depenses

| Intitule | Montant | Categorie | Date (format) |
|----------|---------|-----------|---------------|
| Courses Carrefour | 85.50 | Alimentaire | Date du jour |
| Essence | 65.00 | Transport | Date du jour |
| Restaurant midi | 15.00 | Restaurant | Date du jour |
| Amazon achat | 35.99 | Domotique | Date du jour |
| Medecin | 25.00 | Medical | Date du jour |
| Coiffeur | 22.00 | Hygiene | Date du jour |
| Cadeau anniversaire | 50.00 | Cadeaux | Date du jour |
| Steam jeu | 29.99 | Gaming | Date du jour |

---

## Categories existantes (IDs)

| Categorie | ID |
|-----------|----|
| Pas de Categorie | 1 |
| Energie | 2 |
| Telecom | 1002 |
| Medical | 1003 |
| Domotique | 1004 |
| Travaux | 1006 |
| Travail | 1008 |
| Logement | 1010 |
| Transport | 1011 |
| Restaurant | 1012 |
| Assurances | 1013 |
| Impots | 1014 |
| Education | 1015 |
| Vetements | 1016 |
| Hygiene | 1017 |
| Gaming | 1018 |
| Cadeaux | 1019 |
| Alimentaire | 1020 |

---

## URLs de test

- Application : `http://localhost:5201`
- Page depenses : `http://localhost:5201/depenses`
- Page categories : `http://localhost:5201/categories`
- Swagger API : `http://localhost:5201/swagger`

---

## Endpoints API

| Methode | URL | Description |
|---------|-----|-------------|
| GET | `/api/depensefixe` | Liste depenses fixes |
| POST | `/api/depensefixe` | Ajouter depense fixe |
| PUT | `/api/depensefixe/{id}` | Modifier depense fixe |
| DELETE | `/api/depensefixe/{id}` | Supprimer depense fixe |
| GET | `/api/transaction/revenubymonth/{month}` | Revenus du mois |
| GET | `/api/transaction/depensebymonth/{month}` | Depenses du mois |
| POST | `/api/transaction` | Ajouter transaction |
| GET | `/api/rapport/{annee}/{mois}` | Rapport mensuel |
| GET | `/api/categorie` | Liste categories |
