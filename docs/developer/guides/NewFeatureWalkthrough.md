# New Feature Walkthrough (Domain → Data → API → Tests)

Ship features with confidence by following this thin‑adapter, domain‑first flow.

Goal

- Clear separation of concerns, explicit errors, and strong configuration hygiene.

Overview

1. Design Domain contracts (models, repository/service interfaces, problems)
2. Implement Data (EF entity, mapper, repository)
3. Expose API (DTOs, validators, controller, guards)
4. Wire DI and configuration (Registry keys ↔ YAML)
5. Test (unit + integration)

6. Domain (Why/How)

- Keep pure: no EF/HTTP/DI. Use `CSharp_Result` for flows.
- Define models (records/entities) and repository/service interfaces under `Domain/<Feature>`.
- Add Problems (implement `IDomainProblem`) when domain failures should surface to API.

```csharp
// Domain/<Feature>/IService.cs
public interface IWidgetService {
  Task<Result<Widget?>> GetById(string id);
  Task<Result<Widget>> Create(WidgetRecord record);
}
```

2. Data (Where/How)

- Add EF data model and mapping in `App/Modules/<Feature>/Data`.
- Update DbContext model in `App/StartUp/Database/*` if needed (indexes, relations).
- Implement repository (translate EF exceptions → Problems where appropriate).

```csharp
public class WidgetRepository(MainDbContext db, ILogger<WidgetRepository> log) : IWidgetRepository {
  public async Task<Result<Widget?>> GetById(string id) => (await db.Widgets.FindAsync(id))?.ToDomain();
}
```

3. API (How)

- Add request/response DTOs and FluentValidators under `App/Modules/<Feature>/API/V1`.
- Implement controller with versioned routes, use `AtomiControllerBase` helpers to return results and enforce guards.

```csharp
[ApiVersion(1.0), ApiController]
[Route("api/v{version:apiVersion}/widgets")]
public class WidgetController(IWidgetService svc, IAuthHelper h) : AtomiControllerBase(h) {
  [HttpGet("{id}")]
  public async Task<ActionResult<WidgetRes>> Get(string id) {
    var res = await svc.GetById(id).Then(w => w?.ToRes(), Errors.MapAll);
    return this.ReturnNullableResult(res, new EntityNotFound("Widget Not Found", typeof(Widget), id));
  }
}
```

4. Wire DI & Config (Why/How)

- Register implementations in `App/Modules/DomainServices.cs` using interfaces (`IWidgetService`, `IWidgetRepository`).
- If you need infra (HttpClient, BlockStorage, Cache, SMTP):
  - Add a constant to the appropriate Registry (e.g., `HttpClients.WidgetApi = "WidgetApi"`).
  - Add matching YAML under the same top‑level key (e.g., `HttpClient: WidgetApi: ...`).
  - Always reference via the Registry constant in code (never raw strings).

5. Testing (How)

- Unit tests for validators, mappers, domain services (FluentAssertions).
- Integration tests for controllers (xUnit), run via `pls exec -- dotnet test`.

Checklist

- Domain interfaces + Problems defined
- Repository + Mapping implemented
- API DTOs + Validators + Controller added
- DI wired in `App/Modules/DomainServices.cs`
- Any infra keys added to Registry and YAML (keys match!)
- Tests written and passing

Tips

- Keep the controller thin: validate, guard, call service, map to DTO, return.
- Prefer small PRs that follow this order; they’re easier to review and revert.
