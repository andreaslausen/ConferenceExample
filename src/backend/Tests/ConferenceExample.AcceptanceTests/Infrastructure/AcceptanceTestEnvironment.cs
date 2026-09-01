using Reqnroll;
using Testcontainers.Keycloak;
using Testcontainers.MongoDb;

namespace ConferenceExample.AcceptanceTests.Infrastructure;

/// <summary>
/// Starts a single MongoDB container, a single Keycloak container, and API host for the whole
/// test run — spinning either up per scenario would make the suite far slower than the
/// isolation is worth.
/// </summary>
[Binding]
public class AcceptanceTestEnvironment
{
    private static MongoDbContainer? _mongoContainer;
    private static KeycloakContainer? _keycloakContainer;
    private static AcceptanceTestWebApplicationFactory? _factory;
    private static KeycloakTestUserFactory? _keycloakTestUserFactory;

    public static AcceptanceTestWebApplicationFactory Factory =>
        _factory
        ?? throw new InvalidOperationException(
            "The acceptance test environment has not been started."
        );

    public static KeycloakTestUserFactory KeycloakTestUsers =>
        _keycloakTestUserFactory
        ?? throw new InvalidOperationException(
            "The acceptance test environment has not been started."
        );

    [BeforeTestRun]
    public static async Task StartEnvironment()
    {
        _mongoContainer = new MongoDbBuilder("mongo:8.0").WithReplicaSet().Build();
        _keycloakContainer = new KeycloakBuilder("quay.io/keycloak/keycloak:26.0")
            .WithRealm(
                Path.Combine(AppContext.BaseDirectory, "Infrastructure", "keycloak-realm-test.json")
            )
            .Build();

        await Task.WhenAll(_mongoContainer.StartAsync(), _keycloakContainer.StartAsync());

        var keycloakBaseAddress = _keycloakContainer.GetBaseAddress();
        _keycloakTestUserFactory = new KeycloakTestUserFactory(keycloakBaseAddress);
        _factory = new AcceptanceTestWebApplicationFactory(
            _mongoContainer.GetConnectionString(),
            $"{keycloakBaseAddress}realms/conference-example-test"
        );
    }

    [AfterTestRun]
    public static async Task StopEnvironment()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();
        if (_mongoContainer is not null)
            await _mongoContainer.DisposeAsync();
        if (_keycloakContainer is not null)
            await _keycloakContainer.DisposeAsync();
    }
}
