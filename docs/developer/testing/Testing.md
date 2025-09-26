# Testing (xUnit + FluentAssertions)

Tests should explain behavior and guard against regressions — not mirror implementation.

Why

- Protect behavior during refactors; serve as living documentation.
- FluentAssertions reads like English, making intent obvious.

Where

- Unit tests: `UnitTest/`
- Integration tests: `IntTest/`

Run

```bash
pls exec -- dotnet test
```

Example

```csharp
public class EncryptorTests {
  [Fact]
  public void Roundtrip() {
    var enc = new Encryptor(Options.Create(new EncryptionOption { Secret = "1234567812345678" }));
    var ct = enc.Encrypt("hello");
    enc.Decrypt(ct).Should().Be("hello");
  }
}
```

Tips

- Keep tests focused and deterministic.
- Prefer one Arrange/Act/Assert flow per test for clarity.
- Use integration tests to validate full HTTP flows with in‑memory or test containers when applicable.
