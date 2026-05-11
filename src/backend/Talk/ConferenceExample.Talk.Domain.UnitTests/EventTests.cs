using ConferenceExample.Talk.Domain.TalkManagement.Events;

namespace ConferenceExample.Talk.Domain.UnitTests;

public class EventTests
{
    [Fact]
    public void TalkSubmittedEvent_Constructor_InitializesProperties()
    {
        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;
        var tags = new List<string> { "tag1", "tag2" };
        var speakerId = Guid.CreateVersion7();
        var talkTypeId = Guid.CreateVersion7();
        var conferenceId = Guid.CreateVersion7();

        var @event = new TalkSubmittedEvent(
            aggregateId,
            occurredAt,
            "Test Title",
            "Test Abstract",
            speakerId,
            "Jane",
            "Doe",
            "Speaker bio",
            tags,
            talkTypeId,
            conferenceId,
            "Submitted"
        );

        Assert.Equal(aggregateId, @event.AggregateId);
        Assert.Equal(occurredAt, @event.OccurredAt);
        Assert.Equal("Test Title", @event.Title);
        Assert.Equal("Test Abstract", @event.Abstract);
        Assert.Equal(speakerId, @event.SpeakerId);
        Assert.Equal(tags, @event.Tags);
        Assert.Equal(talkTypeId, @event.TalkTypeId);
        Assert.Equal(conferenceId, @event.ConferenceId);
    }

    [Fact]
    public void TalkSubmittedEvent_Equality_SameValues_ReturnsTrue()
    {
        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;
        var tags = new List<string> { "tag1", "tag2" };
        var speakerId = Guid.CreateVersion7();
        var talkTypeId = Guid.CreateVersion7();
        var conferenceId = Guid.CreateVersion7();

        var event1 = new TalkSubmittedEvent(
            aggregateId,
            occurredAt,
            "Title",
            "Abstract",
            speakerId,
            "Jane",
            "Doe",
            "Speaker bio",
            tags,
            talkTypeId,
            conferenceId,
            "Submitted"
        );

        var event2 = new TalkSubmittedEvent(
            aggregateId,
            occurredAt,
            "Title",
            "Abstract",
            speakerId,
            "Jane",
            "Doe",
            "Speaker bio",
            tags,
            talkTypeId,
            conferenceId,
            "Submitted"
        );

        Assert.Equal(event1, event2);
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
