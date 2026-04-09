using ConferenceExample.EventStore;
using ConferenceExample.Talk.Application;
using ConferenceExample.Talk.Domain.Repositories;
using ConferenceExample.Talk.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll.Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Talk.AcceptanceTests;

public class SetupTestDependencies
{
    [ScenarioDependencies]
    public static IServiceCollection CreateServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IEventStore, InMemoryEventStore>();
        services.AddSingleton<IEventBus, InMemoryEventBus>();
        services.AddScoped<ITalkRepository, TalkRepository>();
        services.AddScoped<ITalkService, TalkService>();
        return services;
    }
}
