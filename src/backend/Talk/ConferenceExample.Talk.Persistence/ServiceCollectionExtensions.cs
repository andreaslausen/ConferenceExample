using ConferenceExample.Talk.Domain.ConferenceManagement;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.TalkManagement;
using ConferenceExample.Talk.Persistence.EventHandlers;
using ConferenceExample.Talk.Persistence.Projections;
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
        services.AddScoped<MongoDbSpeakerReplica>();
        services.AddScoped<ISpeakerReplicaDocumentRepository>(sp =>
            sp.GetRequiredService<MongoDbSpeakerReplica>()
        );
        services.AddScoped<ISpeakerReplica>(sp => sp.GetRequiredService<MongoDbSpeakerReplica>());

        services.AddScoped<MongoDbConferenceReplica>();
        services.AddScoped<IConferenceDocumentRepository>(sp =>
            sp.GetRequiredService<MongoDbConferenceReplica>()
        );
        services.AddScoped<IConferenceReplica>(sp =>
            sp.GetRequiredService<MongoDbConferenceReplica>()
        );

        // Event Handlers
        services.AddScoped<TalkEventHandler>();
        services.AddScoped<TalkSubmissionEventHandler>();
        services.AddScoped<SpeakerReplicaEventHandler>();
        services.AddScoped<ConferenceReplicaEventHandler>();

        return services;
    }
}
