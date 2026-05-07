using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.TalkManagement;
using ConferenceExample.Conference.Persistence.EventHandlers;
using ConferenceExample.Conference.Persistence.ReadModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ConferenceExample.Conference.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConferencePersistence(this IServiceCollection services)
    {
        services.TryAddSingleton<IConferenceEventStore, ConferenceEventStore>();

        // Aggregate Repository (Event Store based) - used by command handlers
        services.AddScoped<IConferenceRepository, ConferenceRepository>();

        // Read Model Repositories (MongoDB based) - used by query handlers
        services.AddScoped<IConferenceDocumentRepository, MongoDbConferenceReadModelRepository>();
        services.AddScoped<IConferenceReadModelRepository>(sp =>
            (IConferenceReadModelRepository)sp.GetRequiredService<IConferenceDocumentRepository>()
        );
        services.AddScoped<
            IConferenceTalkDocumentRepository,
            MongoDbConferenceTalkReadModelRepository
        >();
        services.AddScoped<IConferenceTalkReadModelRepository>(sp =>
            (IConferenceTalkReadModelRepository)
                sp.GetRequiredService<IConferenceTalkDocumentRepository>()
        );

        // Event Handlers
        services.AddScoped<ConferenceEventHandler>();
        services.AddScoped<TalkEventHandler>();

        return services;
    }
}
