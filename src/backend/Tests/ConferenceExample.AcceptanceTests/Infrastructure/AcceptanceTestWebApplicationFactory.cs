using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace ConferenceExample.AcceptanceTests.Infrastructure;

public class AcceptanceTestWebApplicationFactory(string mongoConnectionString)
    : WebApplicationFactory<Program>
{
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
                        ["Jwt:Audience"] = "ConferenceExample.AcceptanceTests",
                        ["Jwt:ExpirationHours"] = "24",
                    }
                );
            }
        );

        builder.ConfigureServices(services =>
        {
            // Replace the configured IMongoDatabase with the Testcontainers instance
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMongoDatabase));
            if (descriptor is not null)
                services.Remove(descriptor);

            var mongoClient = new MongoClient(mongoConnectionString);
            var database = mongoClient.GetDatabase("conference_example_acceptance_tests");
            services.AddSingleton(database);
        });
    }
}
