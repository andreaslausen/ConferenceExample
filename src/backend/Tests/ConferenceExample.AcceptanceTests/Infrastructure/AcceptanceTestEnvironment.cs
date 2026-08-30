using Reqnroll;
using Testcontainers.MongoDb;

namespace ConferenceExample.AcceptanceTests.Infrastructure;

/// <summary>
/// Starts a single MongoDB container and API host for the whole test run — spinning either
/// up per scenario would make the suite far slower than the isolation is worth.
/// </summary>
[Binding]
public class AcceptanceTestEnvironment
{
    private static MongoDbContainer? _mongoContainer;
    private static AcceptanceTestWebApplicationFactory? _factory;

    public static AcceptanceTestWebApplicationFactory Factory =>
        _factory
        ?? throw new InvalidOperationException(
            "The acceptance test environment has not been started."
        );

    [BeforeTestRun]
    public static async Task StartEnvironment()
    {
        _mongoContainer = new MongoDbBuilder("mongo:8.0").WithReplicaSet().Build();
        await _mongoContainer.StartAsync();
        _factory = new AcceptanceTestWebApplicationFactory(_mongoContainer.GetConnectionString());
    }

    [AfterTestRun]
    public static async Task StopEnvironment()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();
        if (_mongoContainer is not null)
            await _mongoContainer.DisposeAsync();
    }
}
