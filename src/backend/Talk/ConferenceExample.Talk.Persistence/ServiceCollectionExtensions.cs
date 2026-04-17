using ConferenceExample.Talk.Domain.TalkManagement;
using ConferenceExample.Talk.Persistence.EventHandlers;
using ConferenceExample.Talk.Persistence.ReadModels;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Talk.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTalkPersistence(this IServiceCollection services)
    {
        // Repositories (Event Store based) - Single Source of Truth for Commands
        services.AddScoped<ITalkRepository, TalkRepository>();
        services.AddScoped<IConferenceRepository, ConferenceRepository>();

        // Read Model Repositories (MongoDB based) - for optimized Queries only
        services.AddScoped<ITalkReadModelRepository, MongoDbTalkReadModelRepository>();

        // Event Handlers - update Read Models when events occur
        services.AddScoped<TalkEventHandler>();

        return services;
    }
}
