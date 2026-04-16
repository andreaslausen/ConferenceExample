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

        // Event Handlers
        services.AddScoped<TalkEventHandler>();

        return services;
    }
}
