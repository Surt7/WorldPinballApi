# Architecture d'Habilitation Dynamique (RBAC / PBAC)

## 1. Contexte & Problématique
Dans une architecture d'entreprise, les autorisations statiques par `enum` ou rôles codés en dur (`[Authorize(Roles = "Admin")]`) présentent une limite majeure :
- Tout ajout ou modification de rôle nécessite de modifier le code C#, recompiler, et redéployer l'API.
- En production, les autorisations sont **Read-Heavy / Write-Rare** : des dizaines de milliers de lectures par seconde sur les requêtes HTTP, contre de très rares modifications administratives.

## 2. Solution Architecturale : Two-Layer / L1 Lock-Free Cache

```
[Requêtes HTTP] ---> [PermissionAuthorizationHandler]
                             | (Lecture L1 nanoseconde - Zéro lock)
                             v
                 [FrozenRolePermissionStore] (RAM)
                 (FrozenDictionary<string, FrozenSet<string>>)
                             ^
                             | (Atomic pointer swap: volatile)
                 [DynamicRolePermissionStore]
                             ^
                             | (Rechargement ponctuel)
                 [PostgreSQL: role_permissions]
```

### A. La Source de Vérité (Base de Données)
- **Table** : `role_permissions` (`role varchar(100)`, `permission varchar(100)`).
- **Clé primaire composite** : `(role, permission)`.
- **Repository** : `IRolePermissionRepository` / `RolePermissionRepository`.

### B. La Projection Mémoire L1 (`FrozenCollections`)
- **`FrozenRolePermissionStore`** :
  - `FrozenDictionary<string, FrozenSet<string>>` avec `StringComparer.OrdinalIgnoreCase`.
  - Double lookup $O(1)$ parfait pré-calculé par le runtime .NET.
  - Zéro allocation GC sur le chemin critique des requêtes.
- **`DynamicRolePermissionStore`** :
  - Maintient une référence `volatile FrozenRolePermissionStore _current`.
  - **Thread-Safety sans verrou** : Le rechargement construit un nouveau store et remplace la référence atomiquement (pointer swap). Aucun thread lecteur n'est bloqué.
  - **Captive Dependency évitée** : Utilisation de `IServiceScopeFactory` dans `ReloadFromDatabaseAsync` pour résoudre le `AppDbContext` (Scoped) depuis le Store (Singleton).

### C. Évaluation dans ASP.NET Core
- **`PermissionRequirement`** : encapsule la permission demandée (ex: `"MachinesProvision"`).
- **`PermissionAuthorizationHandler`** : extrait les claims de rôles (`ClaimTypes.Role`, `"roles"` Keycloak) et interroge le store en mémoire via `Task.CompletedTask`.

### D. Cas d'Usage CQRS (Admin)
- **`GrantPermissionToRoleCommand`** :
  1. Vérifie si l'association existe (`ExistsAsync`).
  2. Insère en BDD (`AddAsync`).
  3. Déclenche le rechargement atomique (`ReloadFromDatabaseAsync`).
- **`RevokePermissionFromRoleCommand`** :
  1. Vérifie si l'association existe (`ExistsAsync`).
  2. Supprime en BDD (`RemoveAsync`).
  3. Déclenche le rechargement atomique (`ReloadFromDatabaseAsync`).

---

## 3. Arguments d'Entretien & Mécanismes sous le capot

1. **Pourquoi `FrozenCollections` plutôt qu'un `ConcurrentDictionary` ?**
   - Un `ConcurrentDictionary` implique un coût de synchronisation de thread et de bucket locking.
   - `FrozenSet` / `FrozenDictionary` fige la table de hachage à l'initialisation. En lecture seule, c'est l'accès le plus rapide possible en .NET, aligné avec les caches CPU L1/L2 et sans aucun lock.
2. **Pourquoi `volatile` ?**
   - Évite la mise en cache de la référence dans un registre CPU local et pose une barrière mémoire (*acquire/release*). Le pointeur mis à jour est instantanément visible sur tous les cœurs CPU.
3. **Comment éviter la Captive Dependency ?**
   - Un Singleton ne doit jamais injecter directement un service Scoped (`AppDbContext`). On injecte `IServiceScopeFactory` pour créer un scope éphémère (`using var scope = ...`) uniquement lors du rechargement.
