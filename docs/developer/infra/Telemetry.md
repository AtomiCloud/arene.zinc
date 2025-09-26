# Telemetry (Logs, Metrics, Traces)

Why

- Capture logs, metrics, and traces for observability. Export locally for development and to OTLP in clusters.

Where it lives

- Services: `App/StartUp/Services/LogsService.cs`, `MetricService.cs`, `TraceService.cs`.
- Options: `App/StartUp/Options/Logging/*`, `Options/Metrics/*`, `Options/Traces/*`.
- Resource: built from `AppOption` (platform, service, module, version, landscape, mode).

Configure (YAML)

```yaml
Logs:
  Exporter:
    Console: { Enabled: true }
    Otlp:
      Enabled: false

Metrics:
  Instrument: { AspNetCore: true, HttpClient: true, Process: true, Runtime: true }
  Exporter:
    Console: { Enabled: false, ExportInterval: 1000 }
    Otlp:
      Enabled: true
      Endpoint: http://localhost:4317
      ProcessorType: Batch

Trace:
  Instrument:
    AspNetCore: { Enabled: true, GrpcSupport: true, RecordException: true }
    HttpClient: { Enabled: true, RecordException: true }
    EFCore: { Enabled: true, SetDbStatementForText: true }
  Exporter:
    Console: { Enabled: true }
```

Validation

```csharp
services.RegisterOption<MetricOption>(MetricOption.Key);
services.RegisterOption<LogsOption>(LogsOption.Key);
services.RegisterOption<TraceOption>(TraceOption.Key);
```

Use

```csharp
services.AddOpenTelemetry()
  .ConfigureResource(rb => ConfigureResourceBuilder(rb))
  .AddMetricService(metrics, meter)
  .AddTraceService(trace);
```

Tips

- Use console exporters locally for fast feedback.
- Prefer OTLP to ship to a collector in dev clusters.
