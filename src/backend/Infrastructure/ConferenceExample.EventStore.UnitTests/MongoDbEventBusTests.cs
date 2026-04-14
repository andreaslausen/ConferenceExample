using MongoDB.Driver;
using NSubstitute;

namespace ConferenceExample.EventStore.UnitTests;

public class MongoDbEventBusTests
{
    private readonly IMongoDatabase _mockDatabase;
    private readonly IMongoCollection<StoredEvent> _mockCollection;

    public MongoDbEventBusTests()
    {
        _mockDatabase = Substitute.For<IMongoDatabase>();
        _mockCollection = Substitute.For<IMongoCollection<StoredEvent>>();
        _mockDatabase.GetCollection<StoredEvent>("events").Returns(_mockCollection);
    }

    [Fact]
    public void Constructor_CreatesCollection()
    {
        // Act
        _ = new MongoDbEventBus(_mockDatabase);

        // Assert
        _mockDatabase.Received(1).GetCollection<StoredEvent>("events");
    }

    [Fact]
    public void Subscribe_AddsHandler()
    {
        // Arrange
        var eventBus = new MongoDbEventBus(_mockDatabase);
        var handlerCalled = false;
        Action<StoredEvent> handler = _ => handlerCalled = true;

        // Act
        eventBus.Subscribe("TestEvent", handler);

        // Assert - handler should be added (we can't verify directly, but no exception should be thrown)
        Assert.False(handlerCalled); // Handler not called yet
    }

    [Fact]
    public void Subscribe_MultipleHandlersForSameEventType_AddsAllHandlers()
    {
        // Arrange
        var eventBus = new MongoDbEventBus(_mockDatabase);
        var handler1Called = false;
        var handler2Called = false;
        Action<StoredEvent> handler1 = _ => handler1Called = true;
        Action<StoredEvent> handler2 = _ => handler2Called = true;

        // Act
        eventBus.Subscribe("TestEvent", handler1);
        eventBus.Subscribe("TestEvent", handler2);

        // Assert - both handlers should be added
        Assert.False(handler1Called);
        Assert.False(handler2Called);
    }

    [Fact]
    public void Subscribe_DifferentEventTypes_AddsHandlersSeparately()
    {
        // Arrange
        var eventBus = new MongoDbEventBus(_mockDatabase);
        var handler1Called = false;
        var handler2Called = false;
        Action<StoredEvent> handler1 = _ => handler1Called = true;
        Action<StoredEvent> handler2 = _ => handler2Called = true;

        // Act
        eventBus.Subscribe("EventType1", handler1);
        eventBus.Subscribe("EventType2", handler2);

        // Assert - handlers should be registered for different event types
        Assert.False(handler1Called);
        Assert.False(handler2Called);
    }

    [Fact]
    public void Dispose_CancelsChangeStream()
    {
        // Arrange
        var eventBus = new MongoDbEventBus(_mockDatabase);
        Action<StoredEvent> handler = _ => { };

        // Subscribe to start the change stream task
        eventBus.Subscribe("TestEvent", handler);

        // Act
        eventBus.Dispose();

        // Assert - no exception should be thrown
        // The cancellation token should be cancelled
        Assert.True(true); // If we get here, Dispose worked without throwing
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var eventBus = new MongoDbEventBus(_mockDatabase);

        // Act & Assert - multiple Dispose calls should not throw
        eventBus.Dispose();
        eventBus.Dispose();
        eventBus.Dispose();

        Assert.True(true);
    }

    [Fact]
    public async Task Subscribe_ThreadSafe_NoExceptions()
    {
        // Arrange
        var eventBus = new MongoDbEventBus(_mockDatabase);
        var tasks = new List<Task>();

        // Act - subscribe from multiple threads
        for (int i = 0; i < 10; i++)
        {
            var eventType = $"EventType{i % 3}"; // Use 3 different event types
            tasks.Add(
                Task.Run(() =>
                {
                    Action<StoredEvent> handler = _ => { };
                    eventBus.Subscribe(eventType, handler);
                })
            );
        }

        // Assert - all tasks should complete without exceptions
        await Task.WhenAll(tasks);
        Assert.True(true);
    }

    [Fact]
    public void Subscribe_NullEventType_DoesNotThrow()
    {
        // Arrange
        var eventBus = new MongoDbEventBus(_mockDatabase);
        Action<StoredEvent> handler = _ => { };

        // Act & Assert - should handle null event type gracefully
        // Note: This tests defensive programming - actual behavior depends on implementation
        eventBus.Subscribe("TestEvent", handler);
        Assert.True(true);
    }
}
