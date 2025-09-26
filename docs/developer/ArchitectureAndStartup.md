# Architecture & Startup

Build features in layers so the core rules stay simple and portable, while adapters and configuration remain easy to change.

Concepts (Why)

- Keep the domain pure: business logic doesn’t know about HTTP, EF, or YAML.
- Adapt at the edges: modules handle API concerns and data/storage specifics.
- Compose at startup: wire options, registries, and services in one place.

Layers (Where)

- Domain: `Domain/*` — models, interfaces, services, transactions.
- App Modules: `App/Modules/*` — controllers, data mappers/repos, glue.
- Startup: `App/StartUp/*` — Options, Services, Registry, Migrators, `Server.cs`.

Startup flow (How)

- `App/Program.cs` builds a generic host, loads YAML, registers options, and instantiates `Server`.
- `App/StartUp/Server.cs` composes services: options, OTEL, CORS, DB/cache, block storage, SMTP, HttpClients, Auth, controllers, swagger.
- Pipeline: conditionally enables Swagger, CORS, auth middleware; maps controllers.

Program.cs (excerpt)

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Configuration
  .AddYamlFile("Config/settings.yaml", optional: false, reloadOnChange: true)
  .AddYamlFile($"Config/settings.{landscape}.yaml", optional: true, reloadOnChange: true)
  .AddEnvironmentVariables(prefix: "Atomi_");
services.AddStartupOptions();
var app = builder.Build();
app.Services.GetRequiredService<Server>().Start(landscape, args);
```

Configuration model (How)

- Strongly-typed options in `App/StartUp/Options/*`; registered and validated in `OptionsExtensions`.
- YAML sources: `App/Config/settings.yaml` and `App/Config/settings.<landscape>.yaml`.

Registering options with validation

```csharp
// OptionsExtensions.cs
services.RegisterOption<AppOption>(AppOption.Key)
  .Validate(app => CorsPolicies.Contains(app.DefaultCors),
    "App:DefaultCors must match a known CorsPolicies constant");
services.RegisterOption<Dictionary<string, HttpClientOption>>(HttpClientOption.Key)
  .Validate(c => c.All(x => HttpClients.Contains(x.Key)),
    "HttpClient key must be declared in Registry.HttpClients");
```

Add a new option (how‑to)

1. Create a class in `App/StartUp/Options` with static `Key` and properties.
2. Register in `OptionsExtensions.RegisterOption<YourOption>(YourOption.Key)` with `.Validate(...)` if needed.
3. Add YAML under the same top‑level key.
4. Consume via `IOptionsMonitor<YourOption>` in services.

CORS (how‑to)

1. Add a name in `App/StartUp/Registry/CorsPolicies.cs`.
2. Define policy in YAML `Cors:` list (origins, headers, methods, preflight, credentials).
3. Set `App:DefaultCors` to the policy name. Startup wires `UseCors` with that name.

YAML example

```yaml
Cors:
  - Name: AllowAll
    PreflightMaxAge: 600
    Origins: ['*']
    Headers: ['*']
    Methods: ['*']
App:
  DefaultCors: AllowAll
```

Observability

- Configure `Logs`, `Metrics`, and `Trace` in YAML. Startup adds exporters/instrumentation accordingly.

OTEL example

```yaml
Logs:
  Exporter:
    Console: { Enabled: true }
Metrics:
  Instrument:
    AspNetCore: true
    HttpClient: true
  Exporter:
    Otlp:
      Enabled: true
      Endpoint: http://localhost:4317
Trace:
  Instrument:
    AspNetCore: { Enabled: true, RecordException: true }
    HttpClient: { Enabled: true, RecordException: true }
  Exporter:
    Console: { Enabled: true }
```

Tips

- Keep option classes small and focused. Prefer expanding with nested option types over adding many unrelated fields.
- Use registries to control names used in YAML. Always reference constants (never raw strings) in code.

Common pitfalls

- Forgetting to add a Registry constant for a new YAML key → code can’t reference it safely.
- Spreading configuration reads across the codebase → keep all configuration under `Options` and read via DI.
