using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.EventStore;
using NSubstitute;
using ConferenceAggregate = ConferenceExample.Conference.Domain.ConferenceManagement.Conference;

namespace ConferenceExample.Conference.Persistence.UnitTests;

public class ConferenceRepositoryTests
{
    private static ConferenceAggregate CreateValidConference(ConferenceId? id = null)
    {
        return ConferenceAggregate.Create(
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
        var eventStore = new TestEventStore();
        var eventBus = new TestEventBus();
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
        var eventStore = new TestEventStore();
        var eventBus = new TestEventBus();
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
        var eventStore = new TestEventStore();
        var eventBus = new TestEventBus();
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
        var eventStore = new TestEventStore();
        var eventBus = new TestEventBus();
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
        var eventStore = new TestEventStore();
        var eventBus = new TestEventBus();
        var repo = new ConferenceRepository(eventStore, eventBus);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.GetById(new ConferenceId(GuidV7.NewGuid()))
        );
    }

    [Fact]
    public async Task GetById_UnknownEventType_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventStore = new TestEventStore();
        var eventBus = new TestEventBus();
        var repo = new ConferenceRepository(eventStore, eventBus);

        var conferenceId = new ConferenceId(GuidV7.NewGuid());

        // Manually insert an event with an unknown type
        var unknownEvent = new StoredEvent(
            Guid.NewGuid(),
            conferenceId.Value,
            "UnknownEventType",
            "{}",
            DateTimeOffset.UtcNow,
            0
        );

        await eventStore.AppendEvents(conferenceId.Value, [unknownEvent], -1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.GetById(conferenceId)
        );
        Assert.Contains("Unknown event type: UnknownEventType", exception.Message);
    }

    [Fact]
    public async Task GetById_InvalidEventPayload_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventStore = new TestEventStore();
        var eventBus = new TestEventBus();
        var repo = new ConferenceRepository(eventStore, eventBus);

        var conferenceId = new ConferenceId(GuidV7.NewGuid());

        // Create an event with null JSON (which will deserialize to null)
        var invalidEvent = new StoredEvent(
            Guid.NewGuid(),
            conferenceId.Value,
            "ConferenceCreatedEvent",
            "null",
            DateTimeOffset.UtcNow,
            0
        );

        await eventStore.AppendEvents(conferenceId.Value, [invalidEvent], -1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.GetById(conferenceId)
        );
        Assert.Contains("Failed to deserialize event: ConferenceCreatedEvent", exception.Message);
    }
}
