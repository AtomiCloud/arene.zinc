# Database (Postgres)

Why

- Persist domain state and query data with EF Core (Npgsql). Explicit keys prevent misconfiguration across environments.

Where it lives

- Contexts: `App/StartUp/Database/*` (e.g., `MainDbContext`).
- Options: `App/StartUp/Options/DatabaseOption.cs`.
- Registry: `App/StartUp/Registry/Databases.cs`.

Define keys

```csharp
// App/StartUp/Registry/Databases.cs
public static class Databases {
  public static readonly Dictionary<string, Type> List = new() {
    { MainDbContext.Key, typeof(MainDbContext) },
  };
  public static IEnumerable<string> AcceptedDatabase() => List.Keys;
}
```

How to add

1. Create a `DbContext` with `public const string Key = "<NAME>";`.
2. Register it in `Databases.List` using that `Key`.
3. Add YAML under `Database:<NAME>` — the `<NAME>` must match your `Key`.
4. Use the context via DI.

Configure (YAML)

```yaml
Database:
  MAIN:
    Host: localhost
    Port: 5432
    Database: aldehyde-manganese
    User: admin
    Password: supersecret
    AutoMigrate: true
    Timeout: 60
```

Key matching

- The YAML key (e.g., `MAIN`) must match the context `Key` and the mapping in `Databases.List`.

Use

- Hosted service `DbMigratorHostedService` runs `DatabaseMigrator` to contact and migrate DBs on startup.
- CLI: `pls migration:create -- <Name>`; run `pls run` to apply.

Example

```csharp
public class UserRepository(MainDbContext db) {
  public Task<UserData?> Find(string id) => db.Users.FirstOrDefaultAsync(x => x.Id == id);
}
```
