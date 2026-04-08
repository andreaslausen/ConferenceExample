using ConferenceExample.EventStore;
using ConferenceExample.Session.Domain.ValueObjects;
using ConferenceExample.Session.Domain.ValueObjects.Ids;
using NSubstitute;
using SessionEntity = ConferenceExample.Session.Domain.Entities.Session;

namespace ConferenceExample.Session.Persistence.UnitTests;

public class SessionRepositoryTests
{
    private static SessionEntity CreateValidSession(ConferenceId? conferenceId = null)
    {
        return SessionEntity.Submit(
            new SessionId(GuidV7.NewGuid()),
            new SessionTitle("Test Title"),
            new SpeakerId(GuidV7.NewGuid()),
            new List<SessionTag> { new("dotnet") },
            new SessionTypeId(GuidV7.NewGuid()),
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
        var repo = new SessionRepository(eventStore, eventBus);
        var session = CreateValidSession();

        // Act
        await repo.Save(session);

        // Assert
        var stored = await eventStore.GetEvents(session.Id.Value);
        Assert.Single(stored);
        Assert.Equal((Guid)session.Id.Value, stored[0].AggregateId);
        Assert.Equal("SessionSubmittedEvent", stored[0].EventType);
    }

    [Fact]
    public async Task Save_NewSession_PublishesEventsToEventBus()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var eventBus = new InMemoryEventBus();
        var repo = new SessionRepository(eventStore, eventBus);
        var session = CreateValidSession();

        var publishedEvents = new List<StoredEvent>();
        eventBus.Subscribe("SessionSubmittedEvent", e => publishedEvents.Add(e));

        // Act
        await repo.Save(session);

        // Assert
        Assert.Single(publishedEvents);
        Assert.Equal((Guid)session.Id.Value, publishedEvents[0].AggregateId);
    }

    [Fact]
    public async Task Save_ClearsUncommittedEventsAfterSaving()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var eventBus = new InMemoryEventBus();
        var repo = new SessionRepository(eventStore, eventBus);
        var session = CreateValidSession();

        // Act
        await repo.Save(session);

        // Assert
        Assert.Empty(session.GetUncommittedEvents());
    }

    [Fact]
    public async Task Save_NoUncommittedEvents_DoesNothing()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var eventBus = Substitute.For<IEventBus>();
        var repo = new SessionRepository(eventStore, eventBus);

        var session = CreateValidSession();
        session.ClearUncommittedEvents();

        // Act
        await repo.Save(session);

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
        var repo = new SessionRepository(eventStore, eventBus);

        var original = CreateValidSession();
        original.EditTitle(new SessionTitle("Updated Title"));
        await repo.Save(original);

        // Act
        var loaded = await repo.GetById(original.Id);

        // Assert
        Assert.Equal(original.Id, loaded.Id);
        Assert.Equal(new SessionTitle("Updated Title"), loaded.Title);
        Assert.Equal(original.ConferenceId, loaded.ConferenceId);
    }

    [Fact]
    public async Task GetById_UnknownSession_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var eventBus = new InMemoryEventBus();
        var repo = new SessionRepository(eventStore, eventBus);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.GetById(new SessionId(GuidV7.NewGuid()))
        );
    }

    [Fact]
    public async Task GetSessions_FiltersSessionsByConferenceId()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var eventBus = new InMemoryEventBus();
        var repo = new SessionRepository(eventStore, eventBus);

        var targetConferenceId = new ConferenceId(GuidV7.NewGuid());
        var sessionA = CreateValidSession(targetConferenceId);
        var sessionB = CreateValidSession();

        await repo.Save(sessionA);
        await repo.Save(sessionB);

        // Act
        var result = await repo.GetSessions(targetConferenceId);

        // Assert
        var single = Assert.Single(result);
        Assert.Equal(sessionA.Id, single.Id);
    }

    [Fact]
    public async Task GetSessions_NoMatchingSessions_ReturnsEmptyList()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var eventBus = new InMemoryEventBus();
        var repo = new SessionRepository(eventStore, eventBus);

        // Act
        var result = await repo.GetSessions(new ConferenceId(GuidV7.NewGuid()));

        // Assert
        Assert.Empty(result);
    }
}
