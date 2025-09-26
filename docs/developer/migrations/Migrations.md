# Migrations (EF Core)

Ship schema changes like code: reviewable, repeatable, and automated.

Why

- Evolve database schema safely, versioned as code.
- Automate application on startup in controlled environments.

Where

- Migrations: `App/Migrations/`.
- Migrator: `App/StartUp/Migrator/DatabaseMigrator.cs`, `DbMigratorHostedService.cs`.

Add migration

```bash
pls migration:create -- AddUserIndex
```

Apply on startup

- Set `Database:<KEY>:AutoMigrate: true` in YAML.
- `DbMigratorHostedService` runs and applies migrations via `DatabaseMigrator`.

Troubleshooting

- Logs show connection attempts and migration status.
- Failures set non‑zero exit code and stop the app.

Notes

- Contexts are registered in `Server.cs` and keys must be present in `Registry.Databases`.
- Avoid destructive changes in auto‑migrate environments; prefer explicit ops runbooks when unsure.
