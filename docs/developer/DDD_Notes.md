# DDD Notes: Aggregates, Principals, Records

Build a mental map of the domain before writing endpoints.

Why

- Keeps business rules independent of infrastructure and easy to test.
- Makes the API a thin adapter rather than the main place where logic lives.

Aggregate Root

- Example: `Domain/User/Model.cs` defines `User` as the aggregate root owning `UserPrincipal` and `UserRecord`.

Entities & Value Objects

- Principal (entity): `UserPrincipal` — identity + core attributes.
- Record (value): `UserRecord` — write model used for create/update flows.

Repositories & Services

- Repository interface in Domain (e.g., `IUserRepository`), implemented in App layer (`App/Modules/Users/Data/*`).
- Domain service (`IUserService`/`UserService`) coordinates repository calls and transactions.

Example

```csharp
// Domain/User/IService.cs
public interface IUserService {
  Task<Result<User?>> GetById(string id);
}

// Domain/User/Service.cs
public class UserService(IUserRepository repo, ITransactionManager tm) : IUserService {
  public Task<Result<User?>> GetById(string id) => repo.GetById(id);
}

// App/Modules/Users/Data/UserRepository.cs
public class UserRepository(MainDbContext db, ILogger<UserRepository> logger) : IUserRepository {
  public async Task<Result<User?>> GetById(string id) =>
    (await db.Users.FirstOrDefaultAsync(x => x.Id == id))?.ToDomain();
}
```

Mapping

- App layer maps between data models (`UserData`) and domain models via mappers (see `UserMapper`).

Guidelines

- Keep domain pure: no HTTP, EF, or DI concerns.
- Use Result to express domain success/failure explicitly.

Registering in DI

```csharp
// App/Modules/DomainServices.cs
services.AddScoped<IUserService, UserService>();
services.AddScoped<IUserRepository, UserRepository>();
```

Tips

- Keep aggregate operations small and transactional.
- Put all mapping between data model and domain model in one place (mapper class).
