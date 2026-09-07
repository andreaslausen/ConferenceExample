using ConferenceExample.Speaker.Domain.SpeakerManagement;
using ConferenceExample.Speaker.Persistence.EventHandlers;
using ConferenceExample.Speaker.Persistence.ReadModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ConferenceExample.Speaker.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSpeakerPersistence(this IServiceCollection services)
    {
        services.TryAddSingleton<ISpeakerEventStore, SpeakerEventStore>();

        // Speaker Aggregate Repository
        services.AddScoped<ISpeakerRepository, SpeakerRepository>();

        // Speaker Read Model Repository (also serves the UserId -> SpeakerId identity lookup)
        services.AddScoped<MongoDbSpeakerReadModelRepository>();
        services.AddScoped<ISpeakerDocumentRepository>(sp =>
            sp.GetRequiredService<MongoDbSpeakerReadModelRepository>()
        );
        services.AddScoped<ISpeakerReadModelRepository>(sp =>
            sp.GetRequiredService<MongoDbSpeakerReadModelRepository>()
        );
        services.AddScoped<ISpeakerLookup>(sp =>
            sp.GetRequiredService<MongoDbSpeakerReadModelRepository>()
        );

        // Event Handlers
        services.AddScoped<SpeakerEventHandler>();

        return services;
    }
}
