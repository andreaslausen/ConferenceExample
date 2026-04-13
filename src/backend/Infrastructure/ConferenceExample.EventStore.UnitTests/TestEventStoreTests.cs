namespace ConferenceExample.EventStore.UnitTests;

public class TestEventStoreTests
{
    private static StoredEvent MakeEvent(Guid aggregateId, long version) =>
        new(Guid.NewGuid(), aggregateId, "TestEvent", "{}", DateTimeOffset.UtcNow, version);

    [Fact]
    public async Task AppendEvents_StoresEvents_GetEventsReturnsThem()
    {
        // Arrange
        var store = new TestEventStore();
        var aggregateId = Guid.NewGuid();
        var events = new[] { MakeEvent(aggregateId, 0) };

        // Act
        await store.AppendEvents(aggregateId, events, expectedVersion: -1);
        var result = await store.GetEvents(aggregateId);

        // Assert
        var stored = Assert.Single(result);
        Assert.Equal(aggregateId, stored.AggregateId);
    }

    [Fact]
    public async Task GetEvents_UnknownAggregate_ReturnsEmptyList()
    {
        // Arrange
        var store = new TestEventStore();

        // Act
        var result = await store.GetEvents(Guid.NewGuid());

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetEvents_ReturnsEventsOrderedByVersion()
    {
        // Arrange
        var store = new TestEventStore();
        var aggregateId = Guid.NewGuid();
        var first = MakeEvent(aggregateId, 0);
        var second = MakeEvent(aggregateId, 1);

        await store.AppendEvents(aggregateId, new[] { first }, expectedVersion: -1);
        await store.AppendEvents(aggregateId, new[] { second }, expectedVersion: 0);

        // Act
        var result = await store.GetEvents(aggregateId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(0, result[0].Version);
        Assert.Equal(1, result[1].Version);
    }

    [Fact]
    public async Task GetAllEvents_ReturnsEventsAcrossMultipleAggregates()
    {
        // Arrange
        var store = new TestEventStore();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        await store.AppendEvents(id1, new[] { MakeEvent(id1, 0) }, expectedVersion: -1);
        await store.AppendEvents(id2, new[] { MakeEvent(id2, 0) }, expectedVersion: -1);

        // Act
        var result = await store.GetAllEvents();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, e => e.AggregateId == id1);
        Assert.Contains(result, e => e.AggregateId == id2);
    }

    [Fact]
    public async Task AppendEvents_VersionMismatch_ThrowsConcurrencyException()
    {
        // Arrange
        var store = new TestEventStore();
        var aggregateId = Guid.NewGuid();

        await store.AppendEvents(
            aggregateId,
            new[] { MakeEvent(aggregateId, 0) },
            expectedVersion: -1
        );

        // Act & Assert
        await Assert.ThrowsAsync<ConcurrencyException>(() =>
            store.AppendEvents(
                aggregateId,
                new[] { MakeEvent(aggregateId, 1) },
                expectedVersion: -1
            )
        );
    }

    [Fact]
    public async Task AppendEvents_MultipleAggregates_KeepsEventsSeparate()
    {
        // Arrange
        var store = new TestEventStore();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        await store.AppendEvents(id1, new[] { MakeEvent(id1, 0) }, expectedVersion: -1);
        await store.AppendEvents(id2, new[] { MakeEvent(id2, 0) }, expectedVersion: -1);

        // Act
        var eventsId1 = await store.GetEvents(id1);
        var eventsId2 = await store.GetEvents(id2);

        // Assert
        Assert.Single(eventsId1);
        Assert.Equal(id1, eventsId1[0].AggregateId);
        Assert.Single(eventsId2);
        Assert.Equal(id2, eventsId2[0].AggregateId);
    }
}
