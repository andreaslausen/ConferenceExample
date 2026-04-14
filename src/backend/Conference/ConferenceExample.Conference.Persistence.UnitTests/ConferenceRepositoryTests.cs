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
            ),
            new OrganizerId(GuidV7.NewGuid())
        );
    }

    [Fact]
    public async Task Save_NewConference_AppendsSerializedEventsToEventStore()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var repo = new ConferenceRepository(eventStore);
        var conference = CreateValidConference();

        // Act
        await repo.Save(conference);

        // Assert
        await eventStore
            .Received(1)
            .AppendEvents(
                conference.Id.Value,
                Arg.Is<IEnumerable<StoredEvent>>(events =>
                    events.Count() == 1
                    && events.First().AggregateId == (Guid)conference.Id.Value
                    && events.First().EventType == "ConferenceCreatedEvent"
                ),
                Arg.Any<long>()
            );
    }

    [Fact]
    public async Task Save_ClearsUncommittedEventsAfterSaving()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var repo = new ConferenceRepository(eventStore);
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
        var repo = new ConferenceRepository(eventStore);

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
        var eventStore = Substitute.For<IEventStore>();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        var organizerId = GuidV7.NewGuid();

        // Create mock events to rebuild the conference
        var start = DateTimeOffset.UtcNow;
        var end = start.AddDays(2);
        var createdEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            conferenceId.Value,
            "ConferenceCreatedEvent",
            $$"""{"AggregateId":"{{conferenceId.Value}}","OccurredAt":"{{DateTimeOffset.UtcNow:O}}","Name":"Test Conference","Start":"{{start:O}}","End":"{{end:O}}","LocationName":"Venue","Street":"Main St 1","City":"Berlin","State":"BE","PostalCode":"10115","Country":"Germany","OrganizerId":"{{organizerId.Value}}"}""",
            DateTimeOffset.UtcNow,
            0
        );
        var renamedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            conferenceId.Value,
            "ConferenceRenamedEvent",
            $$"""{"AggregateId":"{{conferenceId.Value}}","OccurredAt":"{{DateTimeOffset.UtcNow:O}}","Name":"Renamed Conference"}""",
            DateTimeOffset.UtcNow,
            1
        );

        eventStore
            .GetEvents(conferenceId.Value)
            .Returns(new List<StoredEvent> { createdEvent, renamedEvent });

        var repo = new ConferenceRepository(eventStore);

        // Act
        var loaded = await repo.GetById(conferenceId);

        // Assert
        Assert.Equal(conferenceId, loaded.Id);
        Assert.Equal(new Text("Renamed Conference"), loaded.Name);
    }

    [Fact]
    public async Task GetById_UnknownConference_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());

        eventStore.GetEvents(conferenceId.Value).Returns(new List<StoredEvent>());

        var repo = new ConferenceRepository(eventStore);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetById(conferenceId));
    }

    [Fact]
    public async Task GetById_UnknownEventType_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());

        // Mock an event with an unknown type
        var unknownEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            conferenceId.Value,
            "UnknownEventType",
            "{}",
            DateTimeOffset.UtcNow,
            0
        );

        eventStore.GetEvents(conferenceId.Value).Returns(new List<StoredEvent> { unknownEvent });

        var repo = new ConferenceRepository(eventStore);

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
        var eventStore = Substitute.For<IEventStore>();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());

        // Create an event with null JSON (which will deserialize to null)
        var invalidEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            conferenceId.Value,
            "ConferenceCreatedEvent",
            "null",
            DateTimeOffset.UtcNow,
            0
        );

        eventStore.GetEvents(conferenceId.Value).Returns(new List<StoredEvent> { invalidEvent });

        var repo = new ConferenceRepository(eventStore);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.GetById(conferenceId)
        );
        Assert.Contains("Failed to deserialize event: ConferenceCreatedEvent", exception.Message);
    }
}
