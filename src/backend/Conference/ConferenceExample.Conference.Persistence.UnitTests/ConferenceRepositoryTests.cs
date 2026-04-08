using ConferenceExample.Conference.Domain;
using ConferenceExample.Conference.Domain.ValueObjects;
using ConferenceExample.Conference.Domain.ValueObjects.Ids;
using ConferenceExample.EventStore;
using NSubstitute;

namespace ConferenceExample.Conference.Persistence.UnitTests;

public class ConferenceRepositoryTests
{
    private static Domain.Conference CreateValidConference(ConferenceId? id = null)
    {
        return Domain.Conference.Create(
            id ?? new ConferenceId(GuidV7.NewGuid()),
            new Text("Test Conference"),
            new Time(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(2)),
            new Location(
                new Text("Venue"),
                new Address("Main St 1", "Berlin", "BE", "10115", "Germany")
            )
        );
    }

    [Fact]
    public async Task Save_NewConference_AppendsSerializedEventsToEventStore()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var eventBus = new InMemoryEventBus();
        var repo = new ConferenceRepository(eventStore, eventBus);
        var conference = CreateValidConference();

        // Act
        await repo.Save(conference);

        // Assert
        var stored = await eventStore.GetEvents(conference.Id.Value);
        Assert.Single(stored);
        Assert.Equal((Guid)conference.Id.Value, stored[0].AggregateId);
        Assert.Equal("ConferenceCreatedEvent", stored[0].EventType);
    }

    [Fact]
    public async Task Save_NewConference_PublishesEventsToEventBus()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var eventBus = new InMemoryEventBus();
        var repo = new ConferenceRepository(eventStore, eventBus);
        var conference = CreateValidConference();

        var publishedEvents = new List<StoredEvent>();
        eventBus.Subscribe("ConferenceCreatedEvent", e => publishedEvents.Add(e));

        // Act
        await repo.Save(conference);

        // Assert
        Assert.Single(publishedEvents);
        Assert.Equal((Guid)conference.Id.Value, publishedEvents[0].AggregateId);
    }

    [Fact]
    public async Task Save_ClearsUncommittedEventsAfterSaving()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var eventBus = new InMemoryEventBus();
        var repo = new ConferenceRepository(eventStore, eventBus);
        var conference = CreateValidConference();

        // Act
        await repo.Save(conference);

        // Assert
        Assert.Empty(conference.GetUncommittedEvents());
    }

    [Fact]
    public async Task Save_NoUncommittedEvents_DoesNothing()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var eventBus = Substitute.For<IEventBus>();
        var repo = new ConferenceRepository(eventStore, eventBus);

        var conference = CreateValidConference();
        conference.ClearUncommittedEvents();

        // Act
        await repo.Save(conference);

        // Assert
        await eventStore
            .DidNotReceive()
            .AppendEvents(Arg.Any<Guid>(), Arg.Any<IEnumerable<StoredEvent>>(), Arg.Any<long>());
    }

    [Fact]
    public async Task GetById_ExistingConference_RebuildsConferenceFromEvents()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var eventBus = new InMemoryEventBus();
        var repo = new ConferenceRepository(eventStore, eventBus);

        var original = CreateValidConference();
        original.Rename(new Text("Renamed Conference"));
        await repo.Save(original);

        // Act
        var loaded = await repo.GetById(original.Id);

        // Assert
        Assert.Equal(original.Id, loaded.Id);
        Assert.Equal(new Text("Renamed Conference"), loaded.Name);
    }

    [Fact]
    public async Task GetById_UnknownConference_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var eventBus = new InMemoryEventBus();
        var repo = new ConferenceRepository(eventStore, eventBus);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.GetById(new ConferenceId(GuidV7.NewGuid()))
        );
    }
}
