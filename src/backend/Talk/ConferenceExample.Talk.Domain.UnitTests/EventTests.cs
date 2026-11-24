using ConferenceExample.Talk.Domain.TalkManagement.Events;

namespace ConferenceExample.Talk.Domain.UnitTests;

public class EventTests
{
    [Fact]
    public void TalkCreatedEvent_Constructor_InitializesProperties()
    {
        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;
        var speakerId = Guid.CreateVersion7();
        var tags = new List<string> { "tag1", "tag2" };

        var @event = new TalkCreatedEvent(
            aggregateId,
            occurredAt,
            "Test Title",
            "Test Abstract",
            speakerId,
            tags
        );

        Assert.Equal(aggregateId, @event.AggregateId);
        Assert.Equal(occurredAt, @event.OccurredAt);
        Assert.Equal("Test Title", @event.Title);
        Assert.Equal("Test Abstract", @event.Abstract);
        Assert.Equal(speakerId, @event.SpeakerId);
        Assert.Equal(tags, @event.Tags);
    }

    [Fact]
    public void TalkCreatedEvent_Equality_SameValues_ReturnsTrue()
    {
        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;
        var speakerId = Guid.CreateVersion7();
        var tags = new List<string> { "tag1" };

        var event1 = new TalkCreatedEvent(
            aggregateId,
            occurredAt,
            "Title",
            "Abstract",
            speakerId,
            tags
        );
        var event2 = new TalkCreatedEvent(
            aggregateId,
            occurredAt,
            "Title",
            "Abstract",
            speakerId,
            tags
        );

        Assert.Equal(event1, event2);
    }

    [Fact]
    public void TalkSubmittedToConferenceEvent_Constructor_InitializesProperties()
    {
        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;
        var conferenceId = Guid.CreateVersion7();
        var talkTypeId = Guid.CreateVersion7();
        var speakerId = Guid.CreateVersion7();
        var tags = new List<string> { "tag1" };

        var @event = new TalkSubmittedToConferenceEvent(
            aggregateId,
            occurredAt,
            conferenceId,
            talkTypeId,
            speakerId,
            "Title",
            "Abstract",
            tags
        );

        Assert.Equal(aggregateId, @event.AggregateId);
        Assert.Equal(occurredAt, @event.OccurredAt);
        Assert.Equal(conferenceId, @event.ConferenceId);
        Assert.Equal(talkTypeId, @event.TalkTypeId);
        Assert.Equal(speakerId, @event.SpeakerId);
        Assert.Equal("Title", @event.Title);
        Assert.Equal("Abstract", @event.Abstract);
        Assert.Equal(tags, @event.Tags);
    }

    [Fact]
    public void TalkSubmittedToConferenceEvent_Equality_DifferentConference_ReturnsFalse()
    {
        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;
        var talkTypeId = Guid.CreateVersion7();
        var speakerId = Guid.CreateVersion7();

        var event1 = new TalkSubmittedToConferenceEvent(
            aggregateId,
            occurredAt,
            Guid.CreateVersion7(),
            talkTypeId,
            speakerId,
            "Title",
            "Abstract",
            []
        );
        var event2 = new TalkSubmittedToConferenceEvent(
            aggregateId,
            occurredAt,
            Guid.CreateVersion7(),
            talkTypeId,
            speakerId,
            "Title",
            "Abstract",
            []
        );

        Assert.NotEqual(event1, event2);
    }

    [Fact]
    public void TalkDeletedEvent_Constructor_InitializesProperties()
    {
        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;

        var @event = new TalkDeletedEvent(aggregateId, occurredAt);

        Assert.Equal(aggregateId, @event.AggregateId);
        Assert.Equal(occurredAt, @event.OccurredAt);
    }

    [Fact]
    public void TalkTitleEditedEvent_Constructor_InitializesProperties()
    {
        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;

        var @event = new TalkTitleEditedEvent(aggregateId, occurredAt, "Updated Title");

        Assert.Equal(aggregateId, @event.AggregateId);
        Assert.Equal(occurredAt, @event.OccurredAt);
        Assert.Equal("Updated Title", @event.Title);
    }

    [Fact]
    public void TalkAbstractEditedEvent_Constructor_InitializesProperties()
    {
        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;

        var @event = new TalkAbstractEditedEvent(aggregateId, occurredAt, "Updated Abstract");

        Assert.Equal(aggregateId, @event.AggregateId);
        Assert.Equal(occurredAt, @event.OccurredAt);
        Assert.Equal("Updated Abstract", @event.Abstract);
    }

    [Fact]
    public void TalkTagAddedEvent_Constructor_InitializesProperties()
    {
        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;

        var @event = new TalkTagAddedEvent(aggregateId, occurredAt, "newtag");

        Assert.Equal(aggregateId, @event.AggregateId);
        Assert.Equal(occurredAt, @event.OccurredAt);
        Assert.Equal("newtag", @event.Tag);
    }

    [Fact]
    public void TalkTagRemovedEvent_Constructor_InitializesProperties()
    {
        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;

        var @event = new TalkTagRemovedEvent(aggregateId, occurredAt, "removedtag");

        Assert.Equal(aggregateId, @event.AggregateId);
        Assert.Equal(occurredAt, @event.OccurredAt);
        Assert.Equal("removedtag", @event.Tag);
    }
}
