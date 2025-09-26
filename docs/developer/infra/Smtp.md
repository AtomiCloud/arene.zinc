# SMTP (Email Sending)

Why

- Send transactional emails via provider-specific mailboxes.
- You can configure multiple mailboxes; each YAML key must match a constant in `SmtpProviders`.

Where it lives

- Service/Factory: `App/StartUp/Services/SmtpService.cs`, `App/StartUp/Smtp/*`.
- Options: `App/StartUp/Options/SmtpOption.cs`.
- Registry: `App/StartUp/Registry/SmtpProviders.cs`.

Define keys

```csharp
// App/StartUp/Registry/SmtpProviders.cs
public static class SmtpProviders {
  public const string Transactional = "TRANSACTIONAL";
}
```

Configure (YAML)

```yaml
Smtp:
  TRANSACTIONAL:
    Host: smtp.example.com
    Port: 587
    FromEmail: noreply@example.com
    FromName: Example
    Username: user
    Password: pass
    EnableSsl: true
    Timeout: 30000
```

Key matching

- Ensure the YAML key (e.g., `TRANSACTIONAL`) matches `SmtpProviders.Transactional`.
- Add more mailboxes by adding constants and YAML entries (e.g., `MARKETING`).

Use

```csharp
var client = smtpFactory.Get(SmtpProviders.Transactional);
await client.SendAsync(new SmtpEmailMessage {
  To = "user@example.com",
  Subject = "Welcome",
  Body = await renderer.RenderEmail("welcome", new { Name = "Ada" }),
  IsHtml = true
});
```
