# Development & Tasks

Why tasks?

- Reproducible workflows without remembering long commands.
- Safe defaults (env, config) baked into tasks.

Prerequisites

- Nix, direnv, Docker installed.
- Enable the dev shell at repo root:

```bash
direnv allow
```

Core workflow (never run commands naked)

```bash
# install tools, restore nuget, setup email dev deps
pls setup

# start dev mode (Tilt + dotnet watch)
pls dev

# run API once without Tilt
pls run

# create EF Core migration in App/Migrations/
pls migration:create -- AddUsersTable

# preview and build email templates
pls email:dev
pls email:build

# obtain an auth token for local testing
pls auth:token -- --scopes admin

# stop dev or tear down local clusters
pls stop
pls tear

# print latest upstream OCI/chart versions
pls latest

# run tests or any command via Task wrapper
pls exec -- dotnet test
```

Landscapes & config

- App reads `App/Config/settings.yaml` and per‑landscape overlays `App/Config/settings.<landscape>.yaml`.
- Environment variables with prefix `Atomi_` override YAML.
- Dev/Tilt config is in `config/dev.yaml` (mode, ports, cluster settings).

Tips

- Prefer `pls exec -- dotnet test` over raw `dotnet test` (keeps the rule to not run commands naked).
- Use `pls latest` when updating infra dependencies.

Troubleshooting

- If Tilt can’t start, check `config/dev.yaml` and Docker context.
- If migrations fail at boot, verify `Database: <KEY>` matches `Registry.Databases` and credentials.
