using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace ConferenceExample.IntegrationTests.Infrastructure;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder("mongo:8.0")
        .WithReplicaSet()
        .Build();

    private string _mongoConnectionString = string.Empty;

    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();
        _mongoConnectionString = _mongoContainer.GetConnectionString();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(
            (_, config) =>
            {
                config.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["Jwt:Secret"] = "TestSecretKeyMinimum32CharactersLongForHS256Algorithm",
                        ["Jwt:Issuer"] = "ConferenceExample.Tests",
                        ["Jwt:Audience"] = "ConferenceExample.IntegrationTests",
                        ["Jwt:ExpirationHours"] = "24",
                    }
                );
            }
        );

        builder.ConfigureServices(services =>
        {
            // Replace IMongoDatabase with testcontainer instance
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMongoDatabase));
            if (descriptor != null)
                services.Remove(descriptor);

            var mongoClient = new MongoClient(_mongoConnectionString);
            var database = mongoClient.GetDatabase("conference_example_test");
            services.AddSingleton(database);
        });
    }

    public new async Task DisposeAsync()
    {
        await _mongoContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
