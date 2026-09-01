using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace ConferenceExample.AcceptanceTests.Infrastructure;

public class AcceptanceTestWebApplicationFactory(
    string mongoConnectionString,
    string keycloakAuthority
) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Keycloak's test container serves plain HTTP — RequireHttpsMetadata must be off,
        // which the app only does in Development.
        builder.UseEnvironment("Development");

        // UseSetting (not ConfigureAppConfiguration) — with the minimal hosting model,
        // Program.cs reads builder.Configuration before ConfigureAppConfiguration callbacks
        // are merged in, so a later override there would be too late for AddAuthenticationServices.
        builder.UseSetting("Keycloak:Authority", keycloakAuthority);
        builder.UseSetting("Keycloak:Audience", "conference-example-api");

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
