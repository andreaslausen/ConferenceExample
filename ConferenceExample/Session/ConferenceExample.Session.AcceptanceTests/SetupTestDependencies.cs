using ConferenceExample.API;
using ConferenceExample.API.Infrastructure;
using ConferenceExample.Session.Application;
using ConferenceExample.Session.Domain.Repositories;
using ConferenceExample.Session.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll.Microsoft.Extensions.DependencyInjection;
using IDatabaseContext = ConferenceExample.Persistence.IDatabaseContext;

namespace ConferenceExample.Session.AcceptanceTests;

public class SetupTestDependencies
{
    [ScenarioDependencies]
    public static IServiceCollection CreateServices()
    {
        var services = new ServiceCollection();
        var databaseContext = new DatabaseContext();
        services.AddSingleton<IDatabaseContext>(databaseContext);
        services.AddSingleton<Session.Application.IDatabaseContext>(databaseContext);
        services.AddSingleton(databaseContext);
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IIdGenerator, IdGenerator>();
        var counterIdValueGeneratorStrategy = new CounterIdValueGeneratorStrategy();
        services.AddSingleton<IIdValueGeneratorStrategy>(counterIdValueGeneratorStrategy);

        return services;
    }
}