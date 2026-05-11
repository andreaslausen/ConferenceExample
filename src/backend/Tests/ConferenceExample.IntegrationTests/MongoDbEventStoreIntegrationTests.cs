using ConferenceExample.Conference.Persistence;
using ConferenceExample.EventStore;
using ConferenceExample.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace ConferenceExample.IntegrationTests;

[Collection("IntegrationTests")]
public class MongoDbEventStoreIntegrationTests : IntegrationTestBase
{
    private IMongoDatabase _database = null!;

    public MongoDbEventStoreIntegrationTests(IntegrationTestWebApplicationFactory factory)
        : base(factory) { }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _database = Factory.Services.GetRequiredService<IMongoDatabase>();
    }

    [Fact]
    public async Task AppendEvents_WhenVersionIsStale_ThrowsConcurrencyException()
    {
        // Arrange
        var aggregateId = Guid.CreateVersion7();
        var store = new ConferenceEventStore(_database, new InMemoryEventBus());

        var firstEvent = new StoredEvent(
            Guid.CreateVersion7(),
            aggregateId,
            "TestEvent",
            "{}",
            DateTimeOffset.UtcNow,
            0
        );
        await store.AppendEvents(aggregateId, [firstEvent], -1);

        var conflictingEvent = new StoredEvent(
            Guid.CreateVersion7(),
            aggregateId,
            "TestEvent",
            "{}",
            DateTimeOffset.UtcNow,
            1
        );

        // Act & Assert — stale expectedVersion -1, but current version is 0
        await Assert.ThrowsAsync<ConcurrencyException>(() =>
            store.AppendEvents(aggregateId, [conflictingEvent], -1)
        );
    }

    [Fact]
    public async Task AppendEvents_WhenNewAggregateButNonNegativeOneExpectedVersion_ThrowsConcurrencyException()
    {
        // Arrange
        var aggregateId = Guid.CreateVersion7();
        var store = new ConferenceEventStore(_database, new InMemoryEventBus());
        var @event = new StoredEvent(
            Guid.CreateVersion7(),
            aggregateId,
            "TestEvent",
            "{}",
            DateTimeOffset.UtcNow,
            0
        );

        // Act & Assert — new aggregate has no events (version -1), but expectedVersion 0 is wrong
        await Assert.ThrowsAsync<ConcurrencyException>(() =>
            store.AppendEvents(aggregateId, [@event], 0)
        );
    }

    [Fact]
    public async Task AppendEvents_WithCorrectExpectedVersion_Succeeds()
    {
        // Arrange
        var aggregateId = Guid.CreateVersion7();
        var store = new ConferenceEventStore(_database, new InMemoryEventBus());

        var firstEvent = new StoredEvent(
            Guid.CreateVersion7(),
            aggregateId,
            "TestEvent",
            "{}",
            DateTimeOffset.UtcNow,
            0
        );
        await store.AppendEvents(aggregateId, [firstEvent], -1);

        var secondEvent = new StoredEvent(
            Guid.CreateVersion7(),
            aggregateId,
            "TestEvent",
            "{}",
            DateTimeOffset.UtcNow,
            1
        );

        // Act
        await store.AppendEvents(aggregateId, [secondEvent], 0);

        // Assert
        var events = await store.GetEvents(aggregateId);
        Assert.Equal(2, events.Count);
    }
}
