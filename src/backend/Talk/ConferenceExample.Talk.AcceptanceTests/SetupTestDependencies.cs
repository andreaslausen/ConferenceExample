using ConferenceExample.Talk.AcceptanceTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll.Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Talk.AcceptanceTests;

public class SetupTestDependencies
{
    [ScenarioDependencies]
    public static IServiceCollection CreateServices()
    {
        var services = new ServiceCollection();

        // A fresh HttpClient per scenario, backed by the run-wide API host and MongoDB
        // container (see AcceptanceTestEnvironment) — no in-process shortcuts to the
        // application layer, every step goes through the real REST API over HTTP.
        services.AddSingleton(_ => AcceptanceTestEnvironment.Factory.CreateClient());

        return services;
    }
}
