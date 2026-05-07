using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.TalkManagement;
using ConferenceExample.Talk.Persistence.EventHandlers;
using ConferenceExample.Talk.Persistence.ReadModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ConferenceExample.Talk.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTalkPersistence(this IServiceCollection services)
    {
        services.TryAddSingleton<ITalkEventStore, TalkEventStore>();

        // Talk Aggregate Repositories
        services.AddScoped<ITalkRepository, TalkRepository>();
        services.AddScoped<IConferenceRepository, ConferenceRepository>();

        // Talk Read Model Repositories
        services.AddScoped<ITalkDocumentRepository, MongoDbTalkReadModelRepository>();
        services.AddScoped<ITalkReadModelRepository>(sp =>
            (ITalkReadModelRepository)sp.GetRequiredService<ITalkDocumentRepository>()
        );

        // Speaker Aggregate Repository
        services.AddScoped<ISpeakerRepository, SpeakerRepository>();

        // Speaker Read Model Repository
        services.AddScoped<MongoDbSpeakerReadModelRepository>();
        services.AddScoped<ISpeakerDocumentRepository>(sp =>
            sp.GetRequiredService<MongoDbSpeakerReadModelRepository>()
        );
        services.AddScoped<ISpeakerReadModelRepository>(sp =>
            sp.GetRequiredService<MongoDbSpeakerReadModelRepository>()
        );

        // Event Handlers
        services.AddScoped<TalkEventHandler>();
        services.AddScoped<SpeakerEventHandler>();
        services.AddScoped<ConferenceEventReplicationHandler>();

        return services;
    }
}
