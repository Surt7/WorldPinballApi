# WorldPinball API & IoT Platform

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Tests](https://img.shields.io/badge/Tests-496%20Passed%20(100%25)-success)](#testing--quality)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20%7C%20CQRS%20%7C%20DDD-blue)](#architecture)
[![Security](https://img.shields.io/badge/Security-mTLS%20%7C%20ECDSA%20P--256%20%7C%20PBAC-orange)](#security--iot-pipeline)

Plateforme SaaS & IoT haute performance dédiée à la gestion, la télémétrie et le classement temps réel de flippers connectés.
Conçue selon les principes du **Domain-Driven Design (DDD)**, de la **Clean Architecture**, du **CQRS pragmatique** et du **TDD strict**.

---

## Architecture & Flux Système

```mermaid
flowchart TB
    subgraph IoT_Hardware["Machines & Edge (IoT)"]
        Pinball["Machine Connectée\n(TPM 2.0 / P-256)"]
    end

    subgraph Security_Border["Périmètre Sécurité & Réseau"]
        mTLS["Handshake mTLS X.509\n(Kestrel / OS Trust Store)"]
        SigVerif["Vérification Signature\n(ECDSA P-256 / Canonical Payload)"]
    end

    subgraph Application_Core["WorldPinball Core API (.NET 10)"]
        AuthZ["PBAC / RBAC\n(FrozenDictionary Cache)"]
        
        subgraph CQRS["CQRS Pattern"]
            Commands["Command Pipeline\n(EF Core / Aggregats DDD)"]
            Queries["Query Pipeline\n(Dapper / AsNoTracking Projections)"]
        end
    end

    subgraph Infrastructure_Layer["Infrastructure & Persistance"]
        Postgres[("PostgreSQL 16\n(Transactions ACID)")]
        Redis[("Redis\n(Leaderboard / Caching)")]
        RabbitMQ[("RabbitMQ\n(Event-Driven Messaging)")]
    end

    subgraph Frontend_Clients["Clients Web & Back-Office"]
        Angular["Angular 18+ (Signals / Standalone)\nBack-Office & Monitoring"]
    end

    Pinball -->|1. Transport sécurisé mTLS| mTLS
    mTLS -->|2. Header X-Signature| SigVerif
    SigVerif --> AuthZ
    AuthZ --> Commands
    AuthZ --> Queries
    Commands --> Postgres
    Commands --> RabbitMQ
    Queries --> Postgres
    Queries --> Redis
    Angular -->|REST / SignalR| Application_Core
```

---

## Modélisation Métier (Domain-Driven Design)

```mermaid
classDiagram
    direction TB
    class Tournament {
        <<Aggregate Root>>
        +TournamentId Id
        +string Name
        +RegistrationWindow Window
        +TournamentStatus Status
        +OpenRegistration()
        +Start()
        +RecordScore(PlayerId, ScoreValue)
    }

    class Machine {
        <<Aggregate Root>>
        +MachineId Id
        +SerialNumber Serial
        +MachineStatus Status
        +PublicKey CertificateThumbprint
        +Enroll(Thumbprint)
        +Heartbeat()
    }

    class Shipment {
        <<Aggregate Root - Logistics>>
        +ShipmentId Id
        +TrackingNumber Tracking
        +ShipmentStatus Status
        +Address Destination
        +Dispatch()
        +Deliver()
    }

    class ScoreValue {
        <<Value Object>>
        +long Value
        +Validate()
    }

    class RegistrationWindow {
        <<Value Object>>
        +DateTimeOffset StartDate
        +DateTimeOffset EndDate
        +IsOpenAt(DateTimeOffset)
    }

    Tournament *-- ScoreValue : validates
    Tournament *-- RegistrationWindow : encapsulates
```

---

## Choix d'Ingénierie & Décisions Techniques

### Sécurité & Pipeline IoT Anti-Triche
- **Double Barrière de Confiance** :
  - **Couche Transport (mTLS X.509)** : Authentification mutuelle matérielle gérée au niveau Kestrel/Socket.
  - **Couche Applicative (Signature ECDSA P-256)** : Vérification cryptographique de chaque payload de score (`BuildCanonical` : timestamp, nonce, payload hash) pour garantir l'intégrité et la non-répudiation des parties.
- **Autorisations Haute Performance (PBAC)** :
  - Contrôle d'accès basé sur les permissions granulaires.
  - Mise en cache en mémoire via `FrozenDictionary<string, FrozenSet<string>>` garantissant des lookups en O(1) lock-free avec zéro allocation GC.

### CQRS Pragmatique & Performance Mémoire
- **Écriture (Commands)** : Encapsulation stricte du domaine avec Value Objects (`ScoreValue`, `TournamentId`), règles d'invariants et persistance via Entity Framework Core.
- **Lecture (Queries)** : Requêtes Dapper et projections optimisées `.AsNoTracking().Select(...)` sans surcharge d'entités de suivi.
- **Optimisation Mémoire** : Utilisation de `Span<T>` et `ref struct` pour le parsing zero-copy des signatures et buffers.

### Modularité & Bounded Contexts
- Architecture en modules indépendants (Clean Architecture) :
  - `WorldPinball.Api` & `WorldPinball.Core` : Gestion du jeu, tournois et classements.
  - `WorldPinball.Logistics` : Module autonome de gestion du cycle de vie matériel et expéditions.

---

## Qualité Logicielle & TDD

- **Couverture de Tests** : **496 tests automatisés (100% verts)** couvrant le Domaine, l'Application et l'Infrastructure.
- **Standards de Test** :
  - Méthodologie Test-First (TDD Red-Green-Refactor).
  - Structure Roy Osherove AAA (`Arrange`, `Act`, `Assert`).
  - Nommage explicite : `Method_StateUnderTest_ExpectedBehavior`.
  - Isolation via **NSubstitute** et assertions expressives via **FluentAssertions**.

---

## Stack Technique

- **Langage / Framework** : C# 14 / .NET 10 (ASP.NET Core Minimal APIs)
- **Accès Données** : Entity Framework Core 9 / Dapper / Npgsql
- **Bases de Données & Cache** : PostgreSQL, Redis
- **Messaging** : RabbitMQ (AMQP)
- **Sécurité** : mTLS, ECDSA (BouncyCastle / System.Security.Cryptography), Keycloak (OpenID Connect / JWT)
- **Tests** : xUnit, FluentAssertions, NSubstitute, WebApplicationFactory
- **Frontend** : Angular 18+ (Signals, Standalone Components, Reactive)

---

## Démarrage Local

```bash
# 1. Cloner le repository
git clone https://github.com/Surt7/WorldPinballApi.git

# 2. Lancer l'infrastructure (PostgreSQL, RabbitMQ, Redis)
docker compose up -d

# 3. Exécuter l'ensemble de la suite de tests
dotnet test --logger "console;verbosity=normal"

# 4. Lancer l'API
dotnet run --project src/WorldPinball.Api
```
