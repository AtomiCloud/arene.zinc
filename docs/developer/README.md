# Developer Guide

This guide expands on AGENTS.md with concepts, examples, and how‑tos. Use the index below to jump to topics.

Quickstart
```bash
direnv allow         # enable nix dev shell
pls setup            # restore tools and install deps
pls dev              # start Tilt + dotnet watch
# or run server once
pls run
```

Topics
- Project Structure: ./ProjectStructure.md
- Dev & Tasks: ./DevAndTasks.md
- Coding Style: ./CodingStyle.md
- Architecture & Startup: ./ArchitectureAndStartup.md
- Result (pipelines): ./concepts/Result.md
- Problems (RFC7807): ./concepts/Problem.md
- Guards (authorization): ./concepts/Guards.md
- Files (uploads & MIME): ./files/Uploads.md
- Email (SMTP): ./infra/Smtp.md
- Encryption: ./security/Encryption.md
- Transactions: ./Transactions.md
- Testing: ./testing/Testing.md
- Validation: ./validation/Validation.md
- Migrations: ./migrations/Migrations.md
- Commit Conventions: ./CommitConventions.md
- DDD Notes: ./DDD_Notes.md

Infrastructure
- Registry & Keys: ./infra/RegistryAndKeys.md
- Database (Postgres): ./infra/Database.md
- Cache (Redis/Dragonfly): ./infra/Cache.md
- Block Storage (MinIO/S3): ./infra/BlockStorage.md
- HttpClient (Named): ./infra/HttpClient.md
- SMTP (Email): ./infra/Smtp.md
- CORS Policies: ./infra/Cors.md
- Auth Policies & Guards: ./infra/AuthPolicies.md
- Telemetry (OTEL): ./infra/Telemetry.md

Guides
- New Feature Walkthrough: ./guides/NewFeatureWalkthrough.md
 - Define Errors: ./guides/DefineErrors.md

Tip: Never run commands naked. Always use `pls <task>`.

Deprecated aggregate pages were removed in favor of these granular docs.
