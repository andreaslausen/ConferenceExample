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
        var eventStore = new TestEventStore();
        var eventBus = new TestEventBus();
        var repo = new TalkRepository(eventStore, eventBus);
        var talk = CreateValidTalk();

        // Act
        await repo.Save(talk);

        // Assert
        var stored = await eventStore.GetEvents(talk.Id.Value);
        Assert.Single(stored);
        Assert.Equal((Guid)talk.Id.Value, stored[0].AggregateId);
        Assert.Equal("TalkSubmittedEvent", stored[0].EventType);
    }

    [Fact]
    public async Task Save_NewTalk_PublishesEventsToEventBus()
    {
        // Arrange
        var eventStore = new TestEventStore();
        var eventBus = new TestEventBus();
        var repo = new TalkRepository(eventStore, eventBus);
        var talk = CreateValidTalk();

        var publishedEvents = new List<StoredEvent>();
        eventBus.Subscribe("TalkSubmittedEvent", e => publishedEvents.Add(e));

        // Act
        await repo.Save(talk);

        // Assert
        Assert.Single(publishedEvents);
        Assert.Equal((Guid)talk.Id.Value, publishedEvents[0].AggregateId);
    }

    [Fact]
    public async Task Save_ClearsUncommittedEventsAfterSaving()
    {
        // Arrange
        var eventStore = new TestEventStore();
        var eventBus = new TestEventBus();
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
        var eventStore = new TestEventStore();
        var eventBus = new TestEventBus();
        var repo = new TalkRepository(eventStore, eventBus);

        var original = CreateValidTalk();
        original.EditTitle(new TalkTitle("Updated Title"));
        await repo.Save(original);

        // Act
        var loaded = await repo.GetById(original.Id);

        // Assert
        Assert.Equal(original.Id, loaded.Id);
        Assert.Equal(new TalkTitle("Updated Title"), loaded.Title);
        Assert.Equal(original.ConferenceId, loaded.ConferenceId);
    }

    [Fact]
    public async Task GetById_UnknownTalk_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventStore = new TestEventStore();
        var eventBus = new TestEventBus();
        var repo = new TalkRepository(eventStore, eventBus);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.GetById(new TalkId(GuidV7.NewGuid()))
        );
    }

    [Fact]
    public async Task GetTalks_FiltersTalksByConferenceId()
    {
        // Arrange
        var eventStore = new TestEventStore();
        var eventBus = new TestEventBus();
        var repo = new TalkRepository(eventStore, eventBus);

        var targetConferenceId = new ConferenceId(GuidV7.NewGuid());
        var talkA = CreateValidTalk(targetConferenceId);
        var talkB = CreateValidTalk();

        await repo.Save(talkA);
        await repo.Save(talkB);

        // Act
        var result = await repo.GetTalks(targetConferenceId);

        // Assert
        var single = Assert.Single(result);
        Assert.Equal(talkA.Id, single.Id);
    }

    [Fact]
    public async Task GetTalks_NoMatchingTalks_ReturnsEmptyList()
    {
        // Arrange
        var eventStore = new TestEventStore();
        var eventBus = new TestEventBus();
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
