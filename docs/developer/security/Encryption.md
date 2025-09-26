# Encryption (IEncryptor)

Keep small secrets opaque without rolling your own crypto protocols.

Why

- Protect short secrets or tokens while at rest in process or storage fields.
- Not a replacement for KMS or database‑wide encryption.

Where

- Interface/impl: `App/Modules/System/Encryptor.cs`.
- Option: `App/StartUp/Options/EncryptionOption.cs`.

YAML

```yaml
Encryption:
  Secret: 1234567812345678
```

Usage

```csharp
public class Demo(IEncryptor encryptor) {
  public string Roundtrip(string input) {
    var cipher = encryptor.Encrypt(input);
    return encryptor.Decrypt(cipher);
  }
}
```

Testing

```csharp
var enc = new Encryptor(Options.Create(new EncryptionOption { Secret = "1234567812345678" }));
enc.Decrypt(enc.Encrypt("hello")).Should().Be("hello");
```

Notes

- Use 16/24/32 byte keys for AES.
- Not intended for database‑wide encryption or KMS replacement.
- Never log plaintext secrets; log only metadata (length, type) if needed.
