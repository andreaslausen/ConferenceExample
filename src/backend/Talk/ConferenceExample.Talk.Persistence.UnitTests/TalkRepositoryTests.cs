using ConferenceExample.EventStore;
using ConferenceExample.Talk.Domain.ValueObjects;
using ConferenceExample.Talk.Domain.ValueObjects.Ids;
using NSubstitute;
using TalkEntity = ConferenceExample.Talk.Domain.Entities.Talk;

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
    public async Task Save_NewSession_AppendsSerializedEventsToEventStore()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var eventBus = new InMemoryEventBus();
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
    public async Task Save_NewSession_PublishesEventsToEventBus()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var eventBus = new InMemoryEventBus();
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
        var eventStore = new InMemoryEventStore();
        var eventBus = new InMemoryEventBus();
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
    public async Task GetById_ExistingSession_RebuildsSessionFromEvents()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var eventBus = new InMemoryEventBus();
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
    public async Task GetById_UnknownSession_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var eventBus = new InMemoryEventBus();
        var repo = new TalkRepository(eventStore, eventBus);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.GetById(new TalkId(GuidV7.NewGuid()))
        );
    }

    [Fact]
    public async Task GetTalks_FiltersSessionsByConferenceId()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var eventBus = new InMemoryEventBus();
        var repo = new TalkRepository(eventStore, eventBus);

        var targetConferenceId = new ConferenceId(GuidV7.NewGuid());
        var sessionA = CreateValidTalk(targetConferenceId);
        var sessionB = CreateValidTalk();

        await repo.Save(sessionA);
        await repo.Save(sessionB);

        // Act
        var result = await repo.GetTalks(targetConferenceId);

        // Assert
        var single = Assert.Single(result);
        Assert.Equal(sessionA.Id, single.Id);
    }

    [Fact]
    public async Task GetTalks_NoMatchingSessions_ReturnsEmptyList()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var eventBus = new InMemoryEventBus();
        var repo = new TalkRepository(eventStore, eventBus);

        // Act
        var result = await repo.GetTalks(new ConferenceId(GuidV7.NewGuid()));

        // Assert
        Assert.Empty(result);
    }
}
