using ConferenceExample.EventStore;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.TalkManagement;
using NSubstitute;
using TalkEntity = ConferenceExample.Talk.Domain.TalkManagement.Talk;

namespace ConferenceExample.Talk.Persistence.UnitTests;

public class TalkRepositoryTests
{
    private static TalkEntity CreateValidTalk(ConferenceId? conferenceId = null)
    {
        return TalkEntity.Submit(
            new TalkId(GuidV7.NewGuid()),
            new TalkTitle("Test Title"),
            new SpeakerId(GuidV7.NewGuid()),
            new List<TalkTag> { new("dotnet") },
            new TalkTypeId(GuidV7.NewGuid()),
            new Abstract("Test Abstract"),
            conferenceId ?? new ConferenceId(GuidV7.NewGuid())
        );
    }

    [Fact]
    public async Task Save_NewTalk_AppendsSerializedEventsToEventStore()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var eventBus = Substitute.For<IEventBus>();
        var repo = new TalkRepository(eventStore, eventBus);
        var talk = CreateValidTalk();

        // Act
        await repo.Save(talk);

        // Assert
        await eventStore
            .Received(1)
            .AppendEvents(
                talk.Id.Value,
                Arg.Is<IEnumerable<StoredEvent>>(events =>
                    events.Count() == 1
                    && events.First().AggregateId == (Guid)talk.Id.Value
                    && events.First().EventType == "TalkSubmittedEvent"
                ),
                Arg.Any<long>()
            );
    }

    [Fact]
    public async Task Save_NewTalk_PublishesEventsToEventBus()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var eventBus = Substitute.For<IEventBus>();
        var repo = new TalkRepository(eventStore, eventBus);
        var talk = CreateValidTalk();

        // Act
        await repo.Save(talk);

        // Assert
        await eventBus
            .Received(1)
            .Publish(
                Arg.Is<IEnumerable<StoredEvent>>(events =>
                    events.Count() == 1
                    && events.First().AggregateId == (Guid)talk.Id.Value
                    && events.First().EventType == "TalkSubmittedEvent"
                )
            );
    }

    [Fact]
    public async Task Save_ClearsUncommittedEventsAfterSaving()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var eventBus = Substitute.For<IEventBus>();
        var repo = new TalkRepository(eventStore, eventBus);
        var talk = CreateValidTalk();

        // Act
        await repo.Save(talk);

        // Assert
        Assert.Empty(talk.GetUncommittedEvents());
    }

    [Fact]
    public async Task Save_NoUncommittedEvents_DoesNothing()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var eventBus = Substitute.For<IEventBus>();
        var repo = new TalkRepository(eventStore, eventBus);

        var talk = CreateValidTalk();
        talk.ClearUncommittedEvents();

        // Act
        await repo.Save(talk);

        // Assert
        await eventStore
            .DidNotReceive()
            .AppendEvents(Arg.Any<Guid>(), Arg.Any<IEnumerable<StoredEvent>>(), Arg.Any<long>());
    }

    [Fact]
    public async Task GetById_ExistingTalk_RebuildsTalkFromEvents()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var eventBus = Substitute.For<IEventBus>();
        var talkId = new TalkId(GuidV7.NewGuid());
        var speakerId = new SpeakerId(GuidV7.NewGuid());
        var talkTypeId = new TalkTypeId(GuidV7.NewGuid());
        var conferenceId = new ConferenceId(GuidV7.NewGuid());

        // Create mock events to rebuild the talk
        var submittedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId.Value,
            "TalkSubmittedEvent",
            $$"""{"AggregateId":"{{talkId.Value}}","OccurredAt":"{{DateTimeOffset.UtcNow:O}}","Title":"Test Title","Abstract":"Test Abstract","SpeakerId":"{{speakerId.Value}}","Tags":["dotnet"],"TalkTypeId":"{{talkTypeId.Value}}","ConferenceId":"{{conferenceId.Value}}"}""",
            DateTimeOffset.UtcNow,
            0
        );
        var titleEditedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId.Value,
            "TalkTitleEditedEvent",
            $$"""{"AggregateId":"{{talkId.Value}}","OccurredAt":"{{DateTimeOffset.UtcNow:O}}","Title":"Updated Title"}""",
            DateTimeOffset.UtcNow,
            1
        );

        eventStore
            .GetEvents(talkId.Value)
            .Returns(new List<StoredEvent> { submittedEvent, titleEditedEvent });

        var repo = new TalkRepository(eventStore, eventBus);

        // Act
        var loaded = await repo.GetById(talkId);

        // Assert
        Assert.Equal(talkId, loaded.Id);
        Assert.Equal(new TalkTitle("Updated Title"), loaded.Title);
        Assert.Equal(conferenceId, loaded.ConferenceId);
    }

    [Fact]
    public async Task GetById_UnknownTalk_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var eventBus = Substitute.For<IEventBus>();
        var talkId = new TalkId(GuidV7.NewGuid());

        eventStore.GetEvents(talkId.Value).Returns(new List<StoredEvent>());

        var repo = new TalkRepository(eventStore, eventBus);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetById(talkId));
    }

    [Fact]
    public async Task GetTalks_FiltersTalksByConferenceId()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var eventBus = Substitute.For<IEventBus>();
        var repo = new TalkRepository(eventStore, eventBus);

        var targetConferenceId = new ConferenceId(GuidV7.NewGuid());
        var talkAId = new TalkId(GuidV7.NewGuid());
        var talkBId = new TalkId(GuidV7.NewGuid());
        var speakerId = new SpeakerId(GuidV7.NewGuid());
        var talkTypeId = new TalkTypeId(GuidV7.NewGuid());
        var otherConferenceId = new ConferenceId(GuidV7.NewGuid());

        // Create mock events
        var talkAEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkAId.Value,
            "TalkSubmittedEvent",
            $$"""{"AggregateId":"{{talkAId.Value}}","OccurredAt":"{{DateTimeOffset.UtcNow:O}}","Title":"Talk A","Abstract":"Abstract A","SpeakerId":"{{speakerId.Value}}","Tags":["dotnet"],"TalkTypeId":"{{talkTypeId.Value}}","ConferenceId":"{{targetConferenceId.Value}}"}""",
            DateTimeOffset.UtcNow,
            0
        );
        var talkBEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkBId.Value,
            "TalkSubmittedEvent",
            $$"""{"AggregateId":"{{talkBId.Value}}","OccurredAt":"{{DateTimeOffset.UtcNow:O}}","Title":"Talk B","Abstract":"Abstract B","SpeakerId":"{{speakerId.Value}}","Tags":["dotnet"],"TalkTypeId":"{{talkTypeId.Value}}","ConferenceId":"{{otherConferenceId.Value}}"}""",
            DateTimeOffset.UtcNow,
            0
        );

        eventStore.GetAllEvents().Returns(new List<StoredEvent> { talkAEvent, talkBEvent });

        // Act
        var result = await repo.GetTalks(targetConferenceId);

        // Assert
        var single = Assert.Single(result);
        Assert.Equal(talkAId, single.Id);
    }

    [Fact]
    public async Task GetTalks_NoMatchingTalks_ReturnsEmptyList()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var eventBus = Substitute.For<IEventBus>();

        eventStore.GetAllEvents().Returns(new List<StoredEvent>());

        var repo = new TalkRepository(eventStore, eventBus);

        // Act
        var result = await repo.GetTalks(new ConferenceId(GuidV7.NewGuid()));

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetById_UnknownEventType_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var eventBus = new TestEventBus();
        var repo = new TalkRepository(eventStore, eventBus);

        var talkId = new TalkId(GuidV7.NewGuid());
        var invalidEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId.Value,
            "UnknownEventType",
            "{}",
            DateTimeOffset.UtcNow,
            1
        );

        eventStore.GetEvents(talkId.Value).Returns([invalidEvent]);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.GetById(talkId)
        );
        Assert.Contains("Unknown event type: UnknownEventType", exception.Message);
    }

    [Fact]
    public async Task GetById_InvalidPayload_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var eventBus = new TestEventBus();
        var repo = new TalkRepository(eventStore, eventBus);

        var talkId = new TalkId(GuidV7.NewGuid());
        var eventWithInvalidPayload = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId.Value,
            "TalkSubmittedEvent",
            "null",
            DateTimeOffset.UtcNow,
            1
        );

        eventStore.GetEvents(talkId.Value).Returns([eventWithInvalidPayload]);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.GetById(talkId)
        );
        Assert.Contains("Failed to deserialize event: TalkSubmittedEvent", exception.Message);
    }

    [Fact]
    public async Task GetTalks_UnknownEventType_IgnoresUnknownEvents()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var eventBus = new TestEventBus();
        var repo = new TalkRepository(eventStore, eventBus);

        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        var talkId = new TalkId(GuidV7.NewGuid());
        var invalidEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId.Value,
            "UnknownEventType",
            "{}",
            DateTimeOffset.UtcNow,
            1
        );

        eventStore.GetAllEvents().Returns([invalidEvent]);

        // Act
        var result = await repo.GetTalks(conferenceId);

        // Assert
        Assert.Empty(result); // Unknown events are filtered out, so no talks are returned
    }

    [Fact]
    public async Task GetTalks_InvalidPayload_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var eventBus = new TestEventBus();
        var repo = new TalkRepository(eventStore, eventBus);

        var talkId = new TalkId(GuidV7.NewGuid());
        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        var eventWithInvalidPayload = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId.Value,
            "TalkSubmittedEvent",
            "null",
            DateTimeOffset.UtcNow,
            1
        );

        eventStore.GetAllEvents().Returns([eventWithInvalidPayload]);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.GetTalks(conferenceId)
        );
        Assert.Contains("Failed to deserialize event: TalkSubmittedEvent", exception.Message);
    }
}
