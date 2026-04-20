using ConferenceExample.Talk.Domain.TalkManagement;
using ConferenceExample.Talk.Persistence.EventHandlers;
using ConferenceExample.Talk.Persistence.ReadModels;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Talk.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTalkPersistence(this IServiceCollection services)
    {
        // Aggregate Repositories (Event Store based) - used by command handlers
        services.AddScoped<ITalkRepository, TalkRepository>();
        services.AddScoped<IConferenceRepository, ConferenceRepository>();

        // Read Model Repositories (MongoDB based) - used by query handlers
        services.AddScoped<ITalkDocumentRepository, MongoDbTalkReadModelRepository>();
        services.AddScoped<ITalkReadModelRepository>(sp =>
            (ITalkReadModelRepository)sp.GetRequiredService<ITalkDocumentRepository>()
        );

        // Event Handlers - update Read Models when events occur
        services.AddScoped<TalkEventHandler>();

        return services;
    }
}
