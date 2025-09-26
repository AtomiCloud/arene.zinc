# Repository Guidelines

## Project Structure & Modules

- `App/` ASP.NET Core 8 API (controllers, startup, services, migrations, templates).
- `Domain/` DDD core (Aggregates, Principals/Records, services, repositories, Result monad abstractions).
- `App/Modules/*` Application layer per feature (e.g., `Users`, `System`, `Common`).
- `UnitTest/`, `IntTest/` xUnit tests with FluentAssertions.
- `config/`, `infra/`, `tasks/`, `scripts/` Dev infra, Tilt/Helm, and Taskfiles.

## Build, Test & Dev Commands

Always use Taskfile via `pls` (alias for `task`). Examples:

- Setup: `pls setup`
- Dev server (Tilt + watch): `pls dev`
- Run API once: `pls run`
- Create EF migration: `pls migration:create -- <Name>`
- Email templates: `pls email:dev`, `pls email:build`
- Auth token helper: `pls auth:token`
- Stop/Tear down: `pls stop`, `pls tear`
- Tests: `pls exec -- dotnet test` (don’t run naked)
- Lint: `pls lint` (aggregates repo linters when available)

## Coding Style & Naming

- Follow `.editorconfig`: 2‑space indent; organize usings; prefer `var` and braces.
- C# casing: PascalCase for types/members, camelCase for locals/params.
- Keep modules small and feature‑scoped under `App/Modules/<Feature>`.

## Errors, Result, Guards

- Use Result monad (`CSharp_Result`) to compose flows: `.Then`, `.ThenAwait`, `.DoAwait`, `ToResult()`.
- Domain errors are modeled as Problems (implement `IDomainProblem` in `App/Error/V1`). Throw via `new DomainProblemException(problem)`; controllers map to HTTP in `App/Modules/Common/BaseController` and enrich RFC7807 via `ProblemDetailsService`.
- Use guard clauses from `AtomiControllerBase`: `Guard`, `GuardOrAll`, `GuardOrAny` for static policies/dynamic checks.

## Architecture & Startup

- Three layers: Domain (pure), Data (EF/MinIO/Redis), API (controllers). Domain services depend on interfaces (see `Domain/User/*`); implementations live in `App/Modules/*/Data` and are wired in `App/Modules/DomainServices.cs`.
- All runtime config comes from YAML: `App/Config/settings.yaml` (+ `settings.<LANDSCAPE>.yaml`). Extend via `OptionsExtensions` and registries in `App/StartUp/Registry/*`.
- Adding infra:
  - Database: add key in `Registry.Databases`, config under `Database:`, register DbContext, `AutoMigrate` enables migrations.
  - Cache (Redis/Dragonfly): `Cache:` endpoints; DI via `AddCache`.
  - Block storage (MinIO/S3): `BlockStorage:`; use `IFileRepository` and `IFileValidator` for uploads.
  - HttpClients: add key in `Registry.HttpClients`, config under `HttpClient:`; DI via `AddHttpClientService`.
  - SMTP: add mailbox in `Smtp:`, send via `ISmtpClientFactory` and `SmtpEmailMessage`.
  - Auth: `Auth:` settings/policies; use controller guards and `[Authorize(Policy = …)]`.
  - OTEL logs/metrics/traces, CORS, encryption are all toggled via `App/Config` options.
- Transactions: use `ITransactionManager.Start(() => …)` in domain/application flows.

## Testing & Validation

- xUnit + FluentAssertions. Name tests `*Tests` and assert fluently. Run with `pls exec -- dotnet test`.
- Use FluentValidation; prefer `ValidateAsyncResult` helpers to return `ValidationError` problems.

## Commits & PRs

- Conventional Commits enforced by `.gitlint`: types include `feat, fix, docs, style, refactor, perf, test, chore, build, ci, config, dep, amend`.
- PRs: include scope/description, link issues, tests, and screenshots for APIs where helpful.

## Agent-Specific Instructions

- Consult the granular docs in `docs/developer` before implementing features:
  - Architecture/Startup, Result/Problems/Guards, and the Guides (e.g., New Feature Walkthrough).
  - Infra pages (Database, Cache, BlockStorage, HttpClient, SMTP, CORS, Auth, Telemetry) show how to add keys and use Registry constants.
- When adding YAML KVPs (e.g., `HttpClient:`, `BlockStorage:`), define a matching constant in the appropriate Registry and reference the constant in code (never raw strings).
- Always use `pls` tasks; don’t run commands naked. Prefer returning `Result<T>` and mapping Problems via `AtomiControllerBase`.
