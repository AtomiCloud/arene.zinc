using App.Modules.Projects.Data;
using App.Modules.Subscribers.Data;
using App.Modules.SubscriptionTypes.Data;
using App.Modules.System;
using App.Modules.Users.Data;
using App.StartUp.Services;
using Domain;
using Domain.Marketing.Subscribers;
using Domain.Marketing.SubscriptionType;
using Domain.Projects;
using Domain.User;

namespace App.Modules;

public static class DomainServices
{
  public static IServiceCollection AddDomainServices(this IServiceCollection s)
  {
    // USER
    s.AddScoped<IUserService, UserService>()
      .AutoTrace<IUserService>();

    s.AddScoped<IUserRepository, UserRepository>()
      .AutoTrace<IUserRepository>();

    // Project
    s.AddScoped<IProjectService, ProjectService>()
      .AutoTrace<IProjectService>();
    s.AddScoped<IProjectRepository, ProjectRepository>()
      .AutoTrace<IProjectRepository>();

    // Subscription Types
    s.AddScoped<ISubscriptionTypeService, SubscriptionTypeService>()
      .AutoTrace<ISubscriptionTypeService>();
    s.AddScoped<ISubscriptionTypeRepository, SubscriptionTypeRepository>()
      .AutoTrace<ISubscriptionTypeRepository>();

    // Subscribers
    s.AddScoped<ISubscriberService, SubscriberService>()
      .AutoTrace<ISubscriberService>();
    s.AddScoped<ISubscriberRepository, SubscriberRepository>()
      .AutoTrace<ISubscriberRepository>();



    // Transaction Manager
    s.AddScoped<ITransactionManager, TransactionManager>()
      .AutoTrace<ITransactionManager>();

    s.AddScoped<IEncryptor, Encryptor>()
      .AutoTrace<IEncryptor>();



    return s;
  }
}
