using App.Modules;
using App.Modules.System;
using App.StartUp.Database;
using Domain;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IntTest.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<TestProgram>
{
  protected override IHostBuilder CreateHostBuilder()
  {
    // Create a custom host builder that doesn't use Program.cs
    var builder = Host.CreateDefaultBuilder()
      .ConfigureWebHostDefaults(webBuilder =>
      {
        webBuilder.UseTestServer();
        webBuilder.ConfigureServices(services =>
        {
          // Logging
          services.AddLogging(builder =>
          {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
          });

          // Mock database options
          services.AddSingleton<Microsoft.Extensions.Options.IOptionsMonitor<Dictionary<string, App.StartUp.Options.DatabaseOption>>>(
            new MockOptionsMonitor()
          );

          // Database - In-memory
          services.AddDbContext<MainDbContext>((sp, options) =>
          {
            options.UseInMemoryDatabase("TestDb");
            options.EnableSensitiveDataLogging();
            options.UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>());
          });

          // Domain services
          services.AddDomainServices();

          // Transaction manager
          services.AddScoped<ITransactionManager, TransactionManager>();

          // File services (optional - only if tests need them)
          // services.AddMimeDetectionService();
          // services.AddScoped<IFileValidator, FileValidator>();

          // Validators (scan App assembly where controllers are)
          services.AddValidatorsFromAssemblyContaining<MainDbContext>();

          // Controllers - explicitly add from App assembly
          services.AddControllers()
            .AddApplicationPart(typeof(MainDbContext).Assembly);

          services.AddEndpointsApiExplorer();

          // API versioning
          services.AddApiVersioning(options =>
          {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
          });

          // CORS
          services.AddCors(options =>
          {
            options.AddPolicy("AllowAll", builder =>
            {
              builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            });
          });
        });

        webBuilder.Configure(app =>
        {
          // Ensure database is created
          using (var scope = app.ApplicationServices.CreateScope())
          {
            var dbContext = scope.ServiceProvider.GetRequiredService<MainDbContext>();
            dbContext.Database.EnsureCreated();
          }

          app.UseRouting();
          app.UseCors("AllowAll");
          app.UseEndpoints(endpoints =>
          {
            endpoints.MapControllers();
          });
        });
      });

    return builder;
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      try
      {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MainDbContext>();
        dbContext.Database.EnsureDeleted();
      }
      catch
      {
        // Ignore disposal errors
      }
    }

    base.Dispose(disposing);
  }
}

// Mock options monitor to provide database configuration for tests
internal class MockOptionsMonitor : Microsoft.Extensions.Options.IOptionsMonitor<Dictionary<string, App.StartUp.Options.DatabaseOption>>
{
  public Dictionary<string, App.StartUp.Options.DatabaseOption> CurrentValue =>
    new()
    {
      ["MAIN"] = new App.StartUp.Options.DatabaseOption
      {
        Host = "localhost",
        User = "test",
        Password = "test",
        Port = 5432,
        Database = "__IN_MEMORY_TEST__", // Sentinel value to skip PostgreSQL configuration
        AutoMigrate = false,
        Timeout = 30
      }
    };

  public Dictionary<string, App.StartUp.Options.DatabaseOption> Get(string? name) => CurrentValue;

  public IDisposable? OnChange(Action<Dictionary<string, App.StartUp.Options.DatabaseOption>, string?> listener) => null;
}

// Marker class for WebApplicationFactory
public class TestProgram { }
