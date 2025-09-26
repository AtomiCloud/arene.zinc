# Transactions

Concept (Why)

- Ensure multi-step operations either fully succeed or fail without partial side effects.

Implementation (Where)

- Interface: `Domain/ITransactionManager` with `Start<T>(Func<Task<Result<T>>> func)`.
- App impl: `App/Modules/System/TransactionManager.cs` uses `TransactionScope` (RepeatableRead) and logs failures.

Usage (How)

1. Inject `ITransactionManager` into your service/controller.
2. Wrap multi‑step operations:

```csharp
public Task<Result<Unit>> DoStuff() => tm.Start(async () => {
  var a = await repo.Create(...);
  return await a
    .ThenAwait(_ => repo.SyncToExternalSystem(...))
    .Then(_ => new Unit());
});
```

3. Return `Result<T>`; the scope completes only on success.

Notes

- Avoid long‑running operations inside a transaction.
- Use the Result monad to propagate domain failures and prevent partial commits.
