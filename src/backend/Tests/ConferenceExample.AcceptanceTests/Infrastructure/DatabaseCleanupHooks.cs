using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Reqnroll;

namespace ConferenceExample.AcceptanceTests.Infrastructure;

/// <summary>
/// Scenarios share the one Testcontainers-backed MongoDB instance (see
/// AcceptanceTestEnvironment), so each scenario needs a clean database to stay isolated
/// from the last.
/// </summary>
[Binding]
public class DatabaseCleanupHooks
{
    [BeforeScenario]
    public async Task CleanDatabase()
    {
        using var scope = AcceptanceTestEnvironment.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();

        var collectionNames = await (await database.ListCollectionNamesAsync()).ToListAsync();
        foreach (var collectionName in collectionNames)
        {
            await database.DropCollectionAsync(collectionName);
        }
    }
}
