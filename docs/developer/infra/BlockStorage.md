# Block Storage (MinIO/S3-Compatible)

Why

- Persist files (images, attachments) in object storage.
- You can configure multiple stores; each YAML key must match a constant in `BlockStorages`.

Where it lives

- Factory + migrator: `App/StartUp/BlockStorage/*`, `App/StartUp/Migrator/BlockStorageMigrator.cs`.
- Options: `App/StartUp/Options/BlockStorageOption.cs`.
- Registry: `App/StartUp/Registry/BlockStorages.cs`.

Define keys

```csharp
// App/StartUp/Registry/BlockStorages.cs
public static class BlockStorages {
  public const string Main = "MAIN";
}
```

Configure (YAML)

```yaml
BlockStorage:
  MAIN:
    AccessKey: admin
    SecretKey: supersecret
    Bucket: aldehyde-manganese-main-storage
    Read: { Host: localhost, Port: 9000, Scheme: http }
    Write: { Host: localhost, Port: 9000, Scheme: http }
    Policy: Private
    UseSSL: false
    EnsureBucketCreation: true
```

Key matching

- The YAML key `MAIN` must match `BlockStorages.Main`.
- Add more stores by adding more constants and YAML entries (e.g., `ARCHIVE`).

Use

```csharp
// Save a file and return key
var key = await fileRepo.Save(BlockStorages.Main, "user-uploads", fileName, stream, appendExt: true);
```

Why use constants?

- Prevents typos and provides a single source of truth between code and YAML.
