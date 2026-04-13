namespace ConferenceExample.EventStore.UnitTests;

public class TestEventBusTests
{
    private static StoredEvent MakeEvent(string eventType) =>
        new(Guid.NewGuid(), Guid.NewGuid(), eventType, "{}", DateTimeOffset.UtcNow, 0);

    [Fact]
    public async Task Publish_SubscribedHandler_ReceivesEvent()
    {
        // Arrange
        var bus = new TestEventBus();
        StoredEvent? received = null;
        bus.Subscribe("OrderPlaced", e => received = e);
        var storedEvent = MakeEvent("OrderPlaced");

        // Act
        await bus.Publish(new[] { storedEvent });

        // Assert
        Assert.NotNull(received);
        Assert.Equal(storedEvent.Id, received.Id);
    }

    [Fact]
    public async Task Publish_NoSubscribers_DoesNotThrow()
    {
        // Arrange
        var bus = new TestEventBus();
        var storedEvent = MakeEvent("UnknownEvent");

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => bus.Publish(new[] { storedEvent }));
        Assert.Null(exception);
    }

    [Fact]
    public async Task Publish_MultipleSubscribers_AllReceiveEvent()
    {
        // Arrange
        var bus = new TestEventBus();
        var callCount = 0;
        bus.Subscribe("SomethingHappened", _ => callCount++);
        bus.Subscribe("SomethingHappened", _ => callCount++);

        // Act
        await bus.Publish(new[] { MakeEvent("SomethingHappened") });

        // Assert
        Assert.Equal(2, callCount);
    }

    [Fact]
    public async Task Publish_DifferentEventTypes_OnlyMatchingSubscribersReceive()
    {
        // Arrange
        var bus = new TestEventBus();
        var received = new List<StoredEvent>();
        bus.Subscribe("EventA", e => received.Add(e));

        // Act
        await bus.Publish(new[] { MakeEvent("EventA"), MakeEvent("EventB") });

        // Assert
        var single = Assert.Single(received);
        Assert.Equal("EventA", single.EventType);
    }
}
