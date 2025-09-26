# Project Structure

Use this as your map when navigating or adding features.

High‑level layout

```text
App/                     # ASP.NET Core 8 API and adapters
  Config/                # YAML config (settings + landscape overrides)
  Error/                 # Domain Problems and exception wrapper
  Modules/               # Feature modules (Users, System, Common)
  StartUp/               # DI composition, Options, Services, Registry, Migrators
  Templates/Email/       # Email templates (bun) and build outputs
Domain/                  # DDD core: models, services, repositories, transactions
UnitTest/                # Unit tests (xUnit + FluentAssertions)
IntTest/                 # Integration tests (xUnit + FluentAssertions)
infra/, config/, scripts/, tasks/   # Dev infra (Tilt/Helm), helpers, Taskfiles
```

Key patterns

- DDD layering: Domain (pure) → App/Modules (data + API) → StartUp (composition).
- Result monad (`CSharp_Result`) for explicit success/failure flows.
- Problems (`App/Error/V1/*`) surfaced as RFC7807 via `ProblemDetailsService`.
- Strongly‑typed Options in `App/StartUp/Options/*`, validated in `OptionsExtensions`.

When in doubt

- Start at `App/StartUp/Server.cs` to see how the app is wired.
- For a feature, read `Domain/<Feature>` then `App/Modules/<Feature>`.
- For errors and mapping, look at `App/Error/*` and `App/Modules/Common/BaseController.cs`.
