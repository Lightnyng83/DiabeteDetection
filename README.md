# 🩺 Projet 10 - Application de Détection du Diabète (OpenClassrooms)

Ce projet est une solution de **détection de risque de diabète de type 2** construite sur une architecture **microservices .NET**.  
Elle permet à des professionnels de santé de consulter les patients, leurs antécédents et d'évaluer automatiquement leur risque de diabète selon des critères médicaux prédéfinis.

---

## 🧱 Architecture globale

Le projet est divisé en **5 microservices indépendants** :

| Service             | Description                                                                 |
|---------------------|-----------------------------------------------------------------------------|
| **PatientService**  | CRUD des patients, gestion des comptes utilisateurs, JWT Authentification   |
| **NotesService**    | Gestion des notes médicales (stockées dans MongoDB)                         |
| **RiskReportService** | Calcul du niveau de risque basé sur les notes + données du patient        |
| **OcelotGateway**   | API Gateway basé sur Ocelot pour centraliser les appels aux services        |
| **MicroFrontend**   | Frontend ASP.NET MVC (serveur Blazor côté Razor Pages)                      |

---

## 🔑 Identifiants de connexion

Les identifiants utilisés en base pour tester l’authentification sont :

```plaintext
Nom d'utilisateur : testuser
Mot de passe      : Test@12345
```

Ils sont créés automatiquement via les tests d'intégration.

---

## 🧠 Fonctionnement des microservices

### 🔸 PatientService
- Gère les informations personnelles des patients.
- Propose l’enregistrement, la modification, la suppression des patients.
- Authentification basée sur **ASP.NET Identity** + JWT.
- Base de données : **SQL Server**.

### 🔸 NotesService
- Permet d’attacher plusieurs **notes médicales** à un patient.
- Ces notes sont enregistrées dans **MongoDB**.
- Utilise un **DataSeeder** si la collection est vide.

### 🔸 RiskReportService
- Analyse les **notes** + informations personnelles (âge, sexe) pour déterminer le **risque** de diabète.
- Ne stocke rien : il interroge les deux autres services via `HttpClient`.
- 4 niveaux de risque : None, Borderline, In Danger, Early Onset.

### 🔸 OcelotGateway
- Route toutes les requêtes du front vers les bons services backend.
- Permet une communication unifiée et sécurisée entre services.

### 🔸 MicroFrontend
- Interface utilisateur développée en ASP.NET MVC.
- Affiche les patients, leurs notes, le niveau de risque.
- Intègre les appels vers tous les autres services.

---

## 🚀 Lancer le projet avec Docker

> Assurez-vous d’avoir **Docker Desktop** installé.

### Étapes :

```bash
# 1. Cloner le dépôt
git clone https://github.com/votre-utilisateur/Projet10.git
cd Projet10

# 2. Lancer tous les services
docker-compose up --build
```

### Accès :

| Interface        | URL                          |
|------------------|------------------------------|
| Frontend         | http://localhost:5002        |
| API Gateway      | http://localhost:5001        |
| Patients API     | http://localhost:5000/api    |
| Notes API        | http://localhost:5003/api    |
| Risk API         | http://localhost:5004/api    |

---

## 🧪 Tests

### ✅ Tests d’intégration
Un projet `PatientServiceTests` permet de tester :
- L’authentification JWT
- Le contrôleur `AccountController`

> Les tests utilisent **xUnit** et une base **InMemory**.


---

## 💚 Respect du Clean Code

Ce projet respecte les **principes du Clean Code** à travers :

- **Séparation des responsabilités** (SRP) :
  - Chaque Microservice a un rôle clair (ex. `RiskAssessmentService`, `NoteService`, `PatientApiService`).
- **Architecture modulaire et évolutive** :
  - Architecture **microservices** respectée (faible couplage, forte cohésion).
- **Convention de nommage** cohérente : noms explicites et conformes aux conventions .NET.
- **Tests automatisés** pour sécuriser les évolutions.
- **Injection de dépendances** via les interfaces (principes SOLID appliqués).
- **Utilisation de DTOs** pour exposer uniquement les données nécessaires.

---

## 🧾 Annexe : Déclencheurs pour le risque de diabète

Voici les mots-clés recherchés dans les notes pour calculer le risque :

- Hémoglobine A1C
- Microalbumine
- Taille
- Poids
- Fumeur, Fumeuse
- Anormal
- Cholestérol
- Vertiges
- Rechute
- Réaction
- Anticorps

---

## 📌 Remarques

- Tous les services communiquent via des requêtes **HTTP internes** dans le réseau `diabete-net`.
- Les healthchecks sont définis pour **patientservice** et **sqlserver**.
- Le projet est prêt pour un déploiement en **production sur un VPS** via des images Docker et ajout d'un certificat SSL avec Let's Encrypt par exemple.

---

