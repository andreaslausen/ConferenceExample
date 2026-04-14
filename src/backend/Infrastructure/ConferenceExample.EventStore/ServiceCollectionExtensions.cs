using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace ConferenceExample.EventStore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventStore(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Configure MongoDB Event Store
        var mongoSettings =
            configuration.GetSection("EventStore:MongoDB").Get<MongoDbSettings>()
            ?? new MongoDbSettings(); // Use defaults if not configured

        var mongoClient = new MongoClient(mongoSettings.ConnectionString);
        var database = mongoClient.GetDatabase(mongoSettings.DatabaseName);

        services.AddSingleton(database);
        services.AddSingleton<IEventStore, MongoDbEventStore>();
        services.AddSingleton<IEventBus, MongoDbEventBus>();

        return services;
    }
}
