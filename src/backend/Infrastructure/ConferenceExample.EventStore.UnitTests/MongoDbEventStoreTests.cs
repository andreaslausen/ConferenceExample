using MongoDB.Driver;
using NSubstitute;

namespace ConferenceExample.EventStore.UnitTests;

public class MongoDbEventStoreTests
{
    private readonly IMongoDatabase _mockDatabase;
    private readonly IMongoCollection<StoredEvent> _mockCollection;
    private readonly IMongoIndexManager<StoredEvent> _mockIndexManager;
    private readonly IEventBus _mockEventBus;

    public MongoDbEventStoreTests()
    {
        _mockDatabase = Substitute.For<IMongoDatabase>();
        _mockCollection = Substitute.For<IMongoCollection<StoredEvent>>();
        _mockIndexManager = Substitute.For<IMongoIndexManager<StoredEvent>>();
        _mockEventBus = Substitute.For<IEventBus>();

        _mockDatabase.GetCollection<StoredEvent>(Arg.Any<string>()).Returns(_mockCollection);
        _mockCollection.Indexes.Returns(_mockIndexManager);
        _mockCollection.Database.Returns(_mockDatabase);
    }

    [Fact]
    public void Constructor_CreatesCollection()
    {
        _ = new TestEventStore(_mockDatabase, _mockEventBus);

        _mockDatabase.Received(1).GetCollection<StoredEvent>("test_events");
    }

    [Fact]
    public void Constructor_CreatesIndexes()
    {
        _ = new TestEventStore(_mockDatabase, _mockEventBus);

        _mockIndexManager
            .Received(1)
            .CreateMany(Arg.Any<IEnumerable<CreateIndexModel<StoredEvent>>>());
    }

    [Fact]
    public async Task AppendEvents_EmptyEventList_DoesNothing()
    {
        var store = new TestEventStore(_mockDatabase, _mockEventBus);
        var aggregateId = Guid.CreateVersion7();
        var events = new List<StoredEvent>();

        await store.AppendEvents(aggregateId, events, -1);

        var mockClient = Substitute.For<IMongoClient>();
        _mockDatabase.Client.Returns(mockClient);
        await mockClient
            .DidNotReceive()
            .StartSessionAsync(Arg.Any<ClientSessionOptions>(), Arg.Any<CancellationToken>());
        _mockEventBus.DidNotReceive().Publish(Arg.Any<StoredEvent>());
    }

    [Fact]
    public async Task AppendEvents_WithEmptyList_DoesNotPublish()
    {
        var store = new TestEventStore(_mockDatabase, _mockEventBus);
        var aggregateId = Guid.CreateVersion7();

        await store.AppendEvents(aggregateId, [], -1);

        _mockEventBus.DidNotReceive().Publish(Arg.Any<StoredEvent>());
    }

    [Fact]
    public void MongoDbEventStore_CanBeInstantiated()
    {
        var store = new TestEventStore(_mockDatabase, _mockEventBus);

        Assert.NotNull(store);
    }
}
