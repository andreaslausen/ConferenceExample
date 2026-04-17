using ConferenceExample.Talk.Domain.TalkManagement;
using ConferenceExample.Talk.Persistence.EventHandlers;
using ConferenceExample.Talk.Persistence.ReadModels;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Talk.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTalkPersistence(this IServiceCollection services)
    {
        // Repositories (Event Store based)
        services.AddScoped<ITalkRepository, TalkRepository>();

        // Read Model Repositories (MongoDB based)
        services.AddScoped<ITalkReadModelRepository, MongoDbTalkReadModelRepository>();

        // Conference Info Repository - registered as both domain interface and concrete implementation
        // The domain interface (IConferenceInfoRepository) is used by application layer
        // The concrete implementation is used by event handlers that need access to internal methods
        services.AddScoped<MongoDbConferenceReadModelRepository>();
        services.AddScoped<IConferenceInfoRepository>(sp =>
            sp.GetRequiredService<MongoDbConferenceReadModelRepository>()
        );

        // Event Handlers
        services.AddScoped<TalkEventHandler>();
        services.AddScoped<ConferenceEventHandler>();

        return services;
    }
}
