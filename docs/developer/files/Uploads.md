# File Uploads & MIME Validation

Why

- Prevent unsafe uploads and ensure correct content types. Store files reliably in object storage.

Where it lives

- Validator: `App/Modules/Common/IFileValidator.cs`.
- Repository: `App/Modules/Common/FileRepository.cs`.
- Problems: `App/Error/V1/*` (e.g., `InvalidFileType`).

Validate & Save

```csharp
[HttpPost("upload")]
public async Task<ActionResult<string>> Upload(IFormFile file,
  [FromServices] IFileValidator validator,
  [FromServices] IFileRepository repo) {
  await using var stream = file.OpenReadStream();
  var check = await validator.Validate(new StreamContent(stream), new FileValidationParam {
    AllowedMime = new[] { "image/png", "image/jpeg" },
    AllowedExt = new[] { ".png", ".jpg", ".jpeg" }
  });
  // Return Problem automatically if invalid
  var key = await check.ThenAwait(_ => repo.Save(BlockStorages.Main,
    "user-uploads", Path.GetFileNameWithoutExtension(file.FileName), stream, appendExt: true));
  return this.ReturnResult(key);
}
```

Signed Links

```csharp
var url = await repo.SignedLink(BlockStorages.Main, key, seconds: 300);
```

Notes

- `IFileRepository` auto-detects MIME and appends extension when `appendExt: true`.
- Use the Registry constant `BlockStorages.Main` to ensure keys match config.
