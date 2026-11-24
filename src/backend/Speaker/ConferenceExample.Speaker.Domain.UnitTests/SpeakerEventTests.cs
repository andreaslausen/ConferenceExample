using ConferenceExample.Speaker.Domain.SpeakerManagement.Events;

namespace ConferenceExample.Speaker.Domain.UnitTests;

public class SpeakerEventTests
{
    [Fact]
    public void SpeakerProfileCreatedEvent_Constructor_InitializesProperties()
    {
        var aggregateId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;

        var @event = new SpeakerProfileCreatedEvent(
            aggregateId,
            occurredAt,
            userId,
            "Jane",
            "Doe",
            "Speaker bio"
        );

        Assert.Equal(aggregateId, @event.AggregateId);
        Assert.Equal(occurredAt, @event.OccurredAt);
        Assert.Equal(userId, @event.UserId);
        Assert.Equal("Jane", @event.FirstName);
        Assert.Equal("Doe", @event.LastName);
        Assert.Equal("Speaker bio", @event.Biography);
    }

    [Fact]
    public void SpeakerProfileCreatedEvent_Equality_SameValues_ReturnsTrue()
    {
        var aggregateId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;

        var event1 = new SpeakerProfileCreatedEvent(
            aggregateId,
            occurredAt,
            userId,
            "Jane",
            "Doe",
            "Speaker bio"
        );
        var event2 = new SpeakerProfileCreatedEvent(
            aggregateId,
            occurredAt,
            userId,
            "Jane",
            "Doe",
            "Speaker bio"
        );

        Assert.Equal(event1, event2);
    }

    [Fact]
    public void SpeakerProfileUpdatedEvent_Constructor_InitializesProperties()
    {
        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;

        var @event = new SpeakerProfileUpdatedEvent(
            aggregateId,
            occurredAt,
            "John",
            "Smith",
            "Updated bio"
        );

        Assert.Equal(aggregateId, @event.AggregateId);
        Assert.Equal(occurredAt, @event.OccurredAt);
        Assert.Equal("John", @event.FirstName);
        Assert.Equal("Smith", @event.LastName);
        Assert.Equal("Updated bio", @event.Biography);
    }

    [Fact]
    public void SpeakerProfileUpdatedEvent_Equality_DifferentValues_ReturnsFalse()
    {
        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;

        var event1 = new SpeakerProfileUpdatedEvent(
            aggregateId,
            occurredAt,
            "John",
            "Smith",
            "Updated bio"
        );
        var event2 = new SpeakerProfileUpdatedEvent(
            aggregateId,
            occurredAt,
            "John",
            "Smith",
            "Another bio"
        );

        Assert.NotEqual(event1, event2);
    }
}
