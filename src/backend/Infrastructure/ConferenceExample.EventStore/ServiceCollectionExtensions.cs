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
        var mongoSettings =
            configuration.GetSection("Database:MongoDB").Get<MongoDbSettings>()
            ?? new MongoDbSettings();

        var mongoClient = new MongoClient(mongoSettings.ConnectionString);
        var database = mongoClient.GetDatabase(mongoSettings.DatabaseName);

        services.AddSingleton(database);
        services.AddSingleton<MongoDbEventBus>(sp => new MongoDbEventBus(
            sp.GetRequiredService<IMongoDatabase>(),
            ["conference_events", "talk_events"]
        ));
        services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<MongoDbEventBus>());
        services.AddHostedService(sp => sp.GetRequiredService<MongoDbEventBus>());

        return services;
    }
}
