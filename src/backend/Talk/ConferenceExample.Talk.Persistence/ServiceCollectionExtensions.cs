using ConferenceExample.Talk.Domain.ConferenceManagement;
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

        // Talk Aggregate Repository
        services.AddScoped<ITalkRepository, TalkRepository>();

        // Talk Read Model Repository
        services.AddScoped<MongoDbTalkReadModelRepository>();
        services.AddScoped<ITalkDocumentRepository>(sp =>
            sp.GetRequiredService<MongoDbTalkReadModelRepository>()
        );
        services.AddScoped<ITalkReadModelRepository>(sp =>
            sp.GetRequiredService<MongoDbTalkReadModelRepository>()
        );

        // Submission Read Model Repository
        services.AddScoped<MongoDbTalkSubmissionReadModelRepository>();
        services.AddScoped<ITalkSubmissionDocumentRepository>(sp =>
            sp.GetRequiredService<MongoDbTalkSubmissionReadModelRepository>()
        );
        services.AddScoped<ITalkSubmissionReadModelRepository>(sp =>
            sp.GetRequiredService<MongoDbTalkSubmissionReadModelRepository>()
        );

        // Cross-BC projections (Speaker BC, Conference BC)
        services.AddScoped<MongoDbSpeakerDirectory>();
        services.AddScoped<ISpeakerDirectoryDocumentRepository>(sp =>
            sp.GetRequiredService<MongoDbSpeakerDirectory>()
        );
        services.AddScoped<ISpeakerDirectory>(sp =>
            sp.GetRequiredService<MongoDbSpeakerDirectory>()
        );

        services.AddScoped<MongoDbConferenceDirectory>();
        services.AddScoped<IConferenceDocumentRepository>(sp =>
            sp.GetRequiredService<MongoDbConferenceDirectory>()
        );
        services.AddScoped<IConferenceDirectory>(sp =>
            sp.GetRequiredService<MongoDbConferenceDirectory>()
        );

        // Event Handlers
        services.AddScoped<TalkEventHandler>();
        services.AddScoped<TalkSubmissionEventHandler>();
        services.AddScoped<SpeakerDirectoryEventHandler>();
        services.AddScoped<ConferenceDirectoryEventHandler>();

        return services;
    }
}
