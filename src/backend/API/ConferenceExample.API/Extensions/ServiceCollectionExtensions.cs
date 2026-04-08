using ConferenceExample.Conference.Application;
using ConferenceExample.Conference.Domain.Repositories;
using ConferenceExample.Conference.Persistence;
using ConferenceExample.EventStore;
using ConferenceExample.Session.Application;
using ConferenceExample.Session.Domain.Repositories;
using ConferenceExample.Session.Persistence;

namespace ConferenceExample.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IEventStore, InMemoryEventStore>();
        services.AddSingleton<IEventBus, InMemoryEventBus>();

        services.AddScoped<IConferenceRepository, ConferenceRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();

        services.AddScoped<IConferenceService, ConferenceService>();
        services.AddScoped<ISessionService, SessionService>();

        return services;
    }
}
