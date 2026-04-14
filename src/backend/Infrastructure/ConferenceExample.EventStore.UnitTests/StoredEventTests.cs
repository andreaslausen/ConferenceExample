namespace ConferenceExample.EventStore.UnitTests;

public class StoredEventTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var id = Guid.NewGuid();
        var aggregateId = Guid.NewGuid();
        const string eventType = "TestEvent";
        const string payload = "{\"data\": \"test\"}";
        var occurredAt = DateTimeOffset.UtcNow;
        const long version = 1;

        // Act
        var storedEvent = new StoredEvent(id, aggregateId, eventType, payload, occurredAt, version);

        // Assert
        Assert.Equal(id, storedEvent.Id);
        Assert.Equal(aggregateId, storedEvent.AggregateId);
        Assert.Equal(eventType, storedEvent.EventType);
        Assert.Equal(payload, storedEvent.Payload);
        Assert.Equal(occurredAt, storedEvent.OccurredAt);
        Assert.Equal(version, storedEvent.Version);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var aggregateId = Guid.NewGuid();
        const string eventType = "TestEvent";
        const string payload = "{\"data\": \"test\"}";
        var occurredAt = DateTimeOffset.UtcNow;
        const long version = 1;

        var event1 = new StoredEvent(id, aggregateId, eventType, payload, occurredAt, version);
        var event2 = new StoredEvent(id, aggregateId, eventType, payload, occurredAt, version);

        // Act & Assert
        Assert.Equal(event1, event2);
    }

    [Fact]
    public void Equality_DifferentId_AreNotEqual()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        const string eventType = "TestEvent";
        const string payload = "{\"data\": \"test\"}";
        var occurredAt = DateTimeOffset.UtcNow;
        const long version = 1;

        var event1 = new StoredEvent(
            Guid.NewGuid(),
            aggregateId,
            eventType,
            payload,
            occurredAt,
            version
        );
        var event2 = new StoredEvent(
            Guid.NewGuid(),
            aggregateId,
            eventType,
            payload,
            occurredAt,
            version
        );

        // Act & Assert
        Assert.NotEqual(event1, event2);
    }

    [Fact]
    public void Equality_DifferentAggregateId_AreNotEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string eventType = "TestEvent";
        const string payload = "{\"data\": \"test\"}";
        var occurredAt = DateTimeOffset.UtcNow;
        const long version = 1;

        var event1 = new StoredEvent(id, Guid.NewGuid(), eventType, payload, occurredAt, version);
        var event2 = new StoredEvent(id, Guid.NewGuid(), eventType, payload, occurredAt, version);

        // Act & Assert
        Assert.NotEqual(event1, event2);
    }

    [Fact]
    public void Equality_DifferentVersion_AreNotEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var aggregateId = Guid.NewGuid();
        const string eventType = "TestEvent";
        const string payload = "{\"data\": \"test\"}";
        var occurredAt = DateTimeOffset.UtcNow;

        var event1 = new StoredEvent(id, aggregateId, eventType, payload, occurredAt, 1);
        var event2 = new StoredEvent(id, aggregateId, eventType, payload, occurredAt, 2);

        // Act & Assert
        Assert.NotEqual(event1, event2);
    }

    [Fact]
    public void GetHashCode_SameValues_ProduceSameHashCode()
    {
        // Arrange
        var id = Guid.NewGuid();
        var aggregateId = Guid.NewGuid();
        const string eventType = "TestEvent";
        const string payload = "{\"data\": \"test\"}";
        var occurredAt = DateTimeOffset.UtcNow;
        const long version = 1;

        var event1 = new StoredEvent(id, aggregateId, eventType, payload, occurredAt, version);
        var event2 = new StoredEvent(id, aggregateId, eventType, payload, occurredAt, version);

        // Act & Assert
        Assert.Equal(event1.GetHashCode(), event2.GetHashCode());
    }

    [Fact]
    public void With_ModifyingProperty_CreatesNewInstance()
    {
        // Arrange
        var id = Guid.NewGuid();
        var aggregateId = Guid.NewGuid();
        const string eventType = "TestEvent";
        const string payload = "{\"data\": \"test\"}";
        var occurredAt = DateTimeOffset.UtcNow;
        const long version = 1;

        var originalEvent = new StoredEvent(
            id,
            aggregateId,
            eventType,
            payload,
            occurredAt,
            version
        );

        // Act
        var modifiedEvent = originalEvent with
        {
            Version = 2,
        };

        // Assert
        Assert.NotEqual(originalEvent, modifiedEvent);
        Assert.Equal(1, originalEvent.Version);
        Assert.Equal(2, modifiedEvent.Version);
        Assert.Equal(originalEvent.Id, modifiedEvent.Id);
    }

    [Fact]
    public void Constructor_WithNegativeVersion_CreatesInstance()
    {
        // Arrange
        var id = Guid.NewGuid();
        var aggregateId = Guid.NewGuid();
        const string eventType = "TestEvent";
        const string payload = "{}";
        var occurredAt = DateTimeOffset.UtcNow;
        const long version = -1;

        // Act
        var storedEvent = new StoredEvent(id, aggregateId, eventType, payload, occurredAt, version);

        // Assert
        Assert.Equal(version, storedEvent.Version);
    }
}
