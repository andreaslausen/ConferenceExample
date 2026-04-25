using ConferenceExample.Talk.Domain.TalkManagement.Events;

namespace ConferenceExample.Talk.Domain.UnitTests;

public class EventTests
{
    [Fact]
    public void TalkSubmittedEvent_Constructor_InitializesProperties()
    {
        // Arrange
        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;
        var title = "Test Title";
        var @abstract = "Test Abstract";
        var speakerId = Guid.CreateVersion7();
        var tags = new List<string> { "tag1", "tag2" };
        var talkTypeId = Guid.CreateVersion7();
        var conferenceId = Guid.CreateVersion7();

        // Act
        var @event = new TalkSubmittedEvent(
            aggregateId,
            occurredAt,
            1,
            title,
            @abstract,
            speakerId,
            "Jane",
            "Doe",
            "Speaker bio",
            tags,
            talkTypeId,
            conferenceId,
            "Submitted"
        );

        // Assert
        Assert.Equal(aggregateId, @event.AggregateId);
        Assert.Equal(occurredAt, @event.OccurredAt);
        Assert.Equal(title, @event.Title);
        Assert.Equal(@abstract, @event.Abstract);
        Assert.Equal(speakerId, @event.SpeakerId);
        Assert.Equal(tags, @event.Tags);
        Assert.Equal(talkTypeId, @event.TalkTypeId);
        Assert.Equal(conferenceId, @event.ConferenceId);
    }

    [Fact]
    public void TalkSubmittedEvent_Equality_SameValues_ReturnsTrue()
    {
        // Arrange
        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;
        var tags = new List<string> { "tag1", "tag2" };

        var event1 = new TalkSubmittedEvent(
            aggregateId,
            occurredAt,
            1,
            "Title",
            "Abstract",
            Guid.CreateVersion7(),
            "Jane",
            "Doe",
            "Speaker bio",
            tags,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Submitted"
        );

        var event2 = new TalkSubmittedEvent(
            aggregateId,
            occurredAt,
            1,
            "Title",
            "Abstract",
            event1.SpeakerId,
            "Jane",
            "Doe",
            "Speaker bio",
            tags,
            event1.TalkTypeId,
            event1.ConferenceId,
            "Submitted"
        );

        // Act & Assert
        Assert.Equal(event1, event2);
    }

    [Fact]
    public void TalkTitleEditedEvent_Constructor_InitializesProperties()
    {
        // Arrange
        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;
        var title = "Updated Title";

        // Act
        var @event = new TalkTitleEditedEvent(
            aggregateId,
            occurredAt,
            2,
            title,
            "Test Abstract",
            Guid.CreateVersion7(),
            "Jane",
            "Doe",
            "Speaker bio",
            new List<string> { "tag1" },
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Submitted"
        );

        // Assert
        Assert.Equal(aggregateId, @event.AggregateId);
        Assert.Equal(occurredAt, @event.OccurredAt);
        Assert.Equal(title, @event.Title);
    }

    [Fact]
    public void TalkAbstractEditedEvent_Constructor_InitializesProperties()
    {
        // Arrange
        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;
        var @abstract = "Updated Abstract";

        // Act
        var @event = new TalkAbstractEditedEvent(
            aggregateId,
            occurredAt,
            2,
            "Test Title",
            @abstract,
            Guid.CreateVersion7(),
            "Jane",
            "Doe",
            "Speaker bio",
            new List<string> { "tag1" },
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Submitted"
        );

        // Assert
        Assert.Equal(aggregateId, @event.AggregateId);
        Assert.Equal(occurredAt, @event.OccurredAt);
        Assert.Equal(@abstract, @event.Abstract);
    }

    [Fact]
    public void TalkTagAddedEvent_Constructor_InitializesProperties()
    {
        // Arrange
        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;
        var tag = "newtag";

        // Act
        var @event = new TalkTagAddedEvent(
            aggregateId,
            occurredAt,
            2,
            "Test Title",
            "Test Abstract",
            Guid.CreateVersion7(),
            "Jane",
            "Doe",
            "Speaker bio",
            new List<string> { "tag1", tag },
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Submitted"
        );

        // Assert
        Assert.Equal(aggregateId, @event.AggregateId);
        Assert.Equal(occurredAt, @event.OccurredAt);
        Assert.Contains(tag, @event.Tags);
    }

    [Fact]
    public void TalkTagRemovedEvent_Constructor_InitializesProperties()
    {
        // Arrange
        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;
        var tag = "removedtag";

        // Act
        var @event = new TalkTagRemovedEvent(
            aggregateId,
            occurredAt,
            2,
            "Test Title",
            "Test Abstract",
            Guid.CreateVersion7(),
            "Jane",
            "Doe",
            "Speaker bio",
            new List<string>(), // Tag was removed, so it's not in the list
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Submitted"
        );

        // Assert
        Assert.Equal(aggregateId, @event.AggregateId);
        Assert.Equal(occurredAt, @event.OccurredAt);
        Assert.DoesNotContain(tag, @event.Tags);
    }
}
