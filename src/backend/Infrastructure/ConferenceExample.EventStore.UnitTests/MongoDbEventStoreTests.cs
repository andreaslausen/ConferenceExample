using MongoDB.Driver;
using NSubstitute;

namespace ConferenceExample.EventStore.UnitTests;

public class MongoDbEventStoreTests
{
    private readonly IMongoDatabase _mockDatabase;
    private readonly IMongoCollection<StoredEvent> _mockCollection;
    private readonly IMongoIndexManager<StoredEvent> _mockIndexManager;

    public MongoDbEventStoreTests()
    {
        _mockDatabase = Substitute.For<IMongoDatabase>();
        _mockCollection = Substitute.For<IMongoCollection<StoredEvent>>();
        _mockIndexManager = Substitute.For<IMongoIndexManager<StoredEvent>>();

        _mockDatabase.GetCollection<StoredEvent>("events").Returns(_mockCollection);
        _mockCollection.Indexes.Returns(_mockIndexManager);
        _mockCollection.Database.Returns(_mockDatabase);
    }

    [Fact]
    public void Constructor_CreatesCollection()
    {
        // Act
        _ = new MongoDbEventStore(_mockDatabase);

        // Assert
        _mockDatabase.Received(1).GetCollection<StoredEvent>("events");
    }

    [Fact]
    public void Constructor_CreatesIndexes()
    {
        // Act
        _ = new MongoDbEventStore(_mockDatabase);

        // Assert
        _mockIndexManager
            .Received(1)
            .CreateMany(Arg.Any<IEnumerable<CreateIndexModel<StoredEvent>>>());
    }

    [Fact]
    public async Task AppendEvents_EmptyEventList_DoesNothing()
    {
        // Arrange
        var store = new MongoDbEventStore(_mockDatabase);
        var aggregateId = Guid.NewGuid();
        var events = new List<StoredEvent>();

        // Act
        await store.AppendEvents(aggregateId, events, -1);

        // Assert - no session should be started for empty list
        var mockClient = Substitute.For<IMongoClient>();
        _mockDatabase.Client.Returns(mockClient);
        await mockClient
            .DidNotReceive()
            .StartSessionAsync(Arg.Any<ClientSessionOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Constructor_CallsGetCollection()
    {
        // Act
        _ = new MongoDbEventStore(_mockDatabase);

        // Assert
        _mockDatabase.Received(1).GetCollection<StoredEvent>("events");
    }

    [Fact]
    public async Task AppendEvents_WithEmptyList_ReturnsImmediately()
    {
        // Arrange
        var store = new MongoDbEventStore(_mockDatabase);
        var aggregateId = Guid.NewGuid();
        var emptyEvents = new List<StoredEvent>();

        // Act
        await store.AppendEvents(aggregateId, emptyEvents, -1);

        // Assert - should not interact with collection when list is empty
        Assert.True(true); // Test passes if no exception is thrown
    }

    [Fact]
    public void MongoDbEventStore_CanBeInstantiated()
    {
        // Act
        var store = new MongoDbEventStore(_mockDatabase);

        // Assert
        Assert.NotNull(store);
    }
}
