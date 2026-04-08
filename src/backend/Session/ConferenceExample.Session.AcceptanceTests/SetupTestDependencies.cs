using ConferenceExample.EventStore;
using ConferenceExample.Session.Application;
using ConferenceExample.Session.Domain.Repositories;
using ConferenceExample.Session.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll.Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Session.AcceptanceTests;

public class SetupTestDependencies
{
    [ScenarioDependencies]
    public static IServiceCollection CreateServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IEventStore, InMemoryEventStore>();
        services.AddSingleton<IEventBus, InMemoryEventBus>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<ISessionService, SessionService>();
        return services;
    }
}
