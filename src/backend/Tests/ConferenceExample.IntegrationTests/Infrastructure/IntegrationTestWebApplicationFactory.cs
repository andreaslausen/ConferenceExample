using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.MongoDb;

namespace ConferenceExample.IntegrationTests.Infrastructure;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder("mongo:8.0")
        .WithUsername("admin")
        .WithPassword("admin123")
        .Build();

    public string MongoConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();
        MongoConnectionString = _mongoContainer.GetConnectionString();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(
            (_, config) =>
            {
                // Override MongoDB connection string with Testcontainer
                config.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["Database:MongoDB:ConnectionString"] = MongoConnectionString,
                        ["Database:MongoDB:DatabaseName"] = "conference_example_test",
                        ["Jwt:Secret"] = "TestSecretKeyMinimum32CharactersLongForHS256Algorithm",
                        ["Jwt:Issuer"] = "ConferenceExample.Tests",
                        ["Jwt:Audience"] = "ConferenceExample.IntegrationTests",
                        ["Jwt:ExpirationHours"] = "24",
                    }
                );
            }
        );
    }

    public new async Task DisposeAsync()
    {
        await _mongoContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
