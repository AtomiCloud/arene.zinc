using App.Modules.Projects.Data;
using App.Modules.Subscribers.Data;
using App.Modules.SubscriptionTypes.Data;
using App.Modules.Users.Data;
using App.StartUp.Options;
using App.StartUp.Services;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace App.StartUp.Database;

public class MainDbContext(IOptionsMonitor<Dictionary<string, DatabaseOption>> options, ILoggerFactory factory)
  : DbContext
{
  public const string Key = "MAIN";


  public DbSet<UserData> Users { get; set; }
  public DbSet<ProjectData> Projects { get; set; }
  public DbSet<SubscriptionTypeData> SubscriptionTypes { get; set; }
  public DbSet<SubscriberData> Subscribers { get; set; }
  public DbSet<SubscriberTypeStatusData> SubscriberTypeStatuses { get; set; }
  public DbSet<SubscriptionEventData> SubscriptionEvents { get; set; }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    optionsBuilder
      .UseLoggerFactory(factory)
      .AddPostgres(options.CurrentValue, Key)
      .UseExceptionProcessor();
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    var user = modelBuilder.Entity<UserData>();
    user.HasIndex(x => x.Username).IsUnique();

    var project = modelBuilder.Entity<ProjectData>();
    project.HasIndex(x => x.Name);

    var subType = modelBuilder.Entity<SubscriptionTypeData>();
    subType.HasKey(x => new { x.ProjectId, x.Id });
    subType.HasIndex(x => new { x.ProjectId, x.Id }).IsUnique();
    subType.Property(x => x.Id).IsRequired();
    subType
      .HasOne(x => x.Project)
      .WithMany()
      .HasForeignKey(x => x.ProjectId)
      .OnDelete(DeleteBehavior.Cascade);

    var subscriber = modelBuilder.Entity<SubscriberData>();
    subscriber.HasKey(x => new { x.ProjectId, x.Email });
    subscriber.HasIndex(x => new { x.ProjectId, x.Email }).IsUnique();
    subscriber.Property(x => x.Email).IsRequired();
    subscriber
      .HasOne(x => x.Project)
      .WithMany()
      .HasForeignKey(x => x.ProjectId)
      .OnDelete(DeleteBehavior.Cascade);

    var sts = modelBuilder.Entity<SubscriberTypeStatusData>();
    sts.HasKey(x => new { x.ProjectId, x.Email, x.SubscriptionTypeId });
    sts.HasIndex(x => new { x.ProjectId, x.Email, x.SubscriptionTypeId }).IsUnique();
    sts
      .HasOne<SubscriberData>()
      .WithMany()
      .HasForeignKey(x => new { x.ProjectId, x.Email })
      .HasPrincipalKey(x => new { x.ProjectId, x.Email })
      .OnDelete(DeleteBehavior.Cascade);
    sts
      .HasOne<SubscriptionTypeData>()
      .WithMany()
      .HasForeignKey(x => new { x.ProjectId, x.SubscriptionTypeId })
      .HasPrincipalKey(x => new { x.ProjectId, x.Id })
      .OnDelete(DeleteBehavior.Cascade);

    var se = modelBuilder.Entity<SubscriptionEventData>();
    se.HasKey(x => x.Id);
    se.HasIndex(x => new { x.ProjectId, x.Email, x.SubscriptionTypeId, x.Time });
    se
      .HasOne<SubscriberData>()
      .WithMany()
      .HasForeignKey(x => new { x.ProjectId, x.Email })
      .HasPrincipalKey(x => new { x.ProjectId, x.Email })
      .OnDelete(DeleteBehavior.Cascade);
    se
      .HasOne<SubscriptionTypeData>()
      .WithMany()
      .HasForeignKey(x => new { x.ProjectId, x.SubscriptionTypeId })
      .HasPrincipalKey(x => new { x.ProjectId, x.Id })
      .OnDelete(DeleteBehavior.Cascade);
  }
}
