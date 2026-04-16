using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.TalkManagement;
using ConferenceExample.Conference.Persistence.EventHandlers;
using ConferenceExample.Conference.Persistence.ReadModels;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Conference.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConferencePersistence(this IServiceCollection services)
    {
        // Repositories (Event Store based)
        services.AddScoped<IConferenceRepository, ConferenceRepository>();
        services.AddScoped<ITalkRepository, TalkRepository>();

        // Read Model Repositories (MongoDB based)
        services.AddScoped<IConferenceReadModelRepository, MongoDbConferenceReadModelRepository>();
        services.AddScoped<
            IConferenceTalkReadModelRepository,
            MongoDbConferenceTalkReadModelRepository
        >();

        // Event Handlers
        services.AddScoped<ConferenceEventHandler>();
        services.AddScoped<TalkEventHandler>();

        return services;
    }
}
