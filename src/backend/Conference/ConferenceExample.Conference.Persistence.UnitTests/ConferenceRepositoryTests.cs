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

    [Fact]
    public async Task GetAll_WithMultipleConferences_ReturnsAllConferences()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var conferenceId1 = new ConferenceId(GuidV7.NewGuid());
        var conferenceId2 = new ConferenceId(GuidV7.NewGuid());
        var organizerId1 = GuidV7.NewGuid();
        var organizerId2 = GuidV7.NewGuid();

        var start1 = DateTimeOffset.UtcNow;
        var end1 = start1.AddDays(2);
        var start2 = DateTimeOffset.UtcNow.AddMonths(1);
        var end2 = start2.AddDays(3);

        var createdEvent1 = new StoredEvent(
            GuidV7.NewGuid().Value,
            conferenceId1.Value,
            "ConferenceCreatedEvent",
            $$"""{"AggregateId":"{{conferenceId1.Value}}","OccurredAt":"{{DateTimeOffset.UtcNow:O}}","Name":"Conference 1","Start":"{{start1:O}}","End":"{{end1:O}}","LocationName":"Venue 1","Street":"Street 1","City":"City 1","State":"State 1","PostalCode":"12345","Country":"Country 1","OrganizerId":"{{organizerId1.Value}}"}""",
            DateTimeOffset.UtcNow,
            0
        );

        var createdEvent2 = new StoredEvent(
            GuidV7.NewGuid().Value,
            conferenceId2.Value,
            "ConferenceCreatedEvent",
            $$"""{"AggregateId":"{{conferenceId2.Value}}","OccurredAt":"{{DateTimeOffset.UtcNow:O}}","Name":"Conference 2","Start":"{{start2:O}}","End":"{{end2:O}}","LocationName":"Venue 2","Street":"Street 2","City":"City 2","State":"State 2","PostalCode":"67890","Country":"Country 2","OrganizerId":"{{organizerId2.Value}}"}""",
            DateTimeOffset.UtcNow,
            0
        );

        eventStore.GetAllEvents().Returns(new List<StoredEvent> { createdEvent1, createdEvent2 });

        var repo = new ConferenceRepository(eventStore);

        // Act
        var result = await repo.GetAll();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Name.Value == "Conference 1");
        Assert.Contains(result, c => c.Name.Value == "Conference 2");
    }

    [Fact]
    public async Task GetAll_WithNoConferences_ReturnsEmptyList()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        eventStore.GetAllEvents().Returns(new List<StoredEvent>());

        var repo = new ConferenceRepository(eventStore);

        // Act
        var result = await repo.GetAll();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAll_WithNonConferenceEvents_SkipsThem()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        var organizerId = GuidV7.NewGuid();
        var talkId = Guid.NewGuid();

        var start = DateTimeOffset.UtcNow;
        var end = start.AddDays(2);

        var conferenceCreatedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            conferenceId.Value,
            "ConferenceCreatedEvent",
            $$"""{"AggregateId":"{{conferenceId.Value}}","OccurredAt":"{{DateTimeOffset.UtcNow:O}}","Name":"Test Conference","Start":"{{start:O}}","End":"{{end:O}}","LocationName":"Venue","Street":"Main St 1","City":"Berlin","State":"BE","PostalCode":"10115","Country":"Germany","OrganizerId":"{{organizerId.Value}}"}""",
            DateTimeOffset.UtcNow,
            0
        );

        // Some non-conference event (e.g., from Talk BC)
        var talkSubmittedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId,
            "TalkSubmittedEvent",
            $$"""{"Title":"Some Talk","Abstract":"Abstract","SpeakerId":"{{Guid.NewGuid()}}","Tags":[],"TalkTypeId":"{{Guid.NewGuid()}}","ConferenceId":"{{conferenceId.Value}}"}""",
            DateTimeOffset.UtcNow,
            0
        );

        eventStore
            .GetAllEvents()
            .Returns(new List<StoredEvent> { conferenceCreatedEvent, talkSubmittedEvent });

        var repo = new ConferenceRepository(eventStore);

        // Act
        var result = await repo.GetAll();

        // Assert
        Assert.Single(result);
        Assert.Equal("Test Conference", result[0].Name.Value);
    }

    [Fact]
    public async Task GetAll_WithConferenceUpdates_ReturnsUpdatedConferences()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        var organizerId = GuidV7.NewGuid();

        var start = DateTimeOffset.UtcNow;
        var end = start.AddDays(2);

        var createdEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            conferenceId.Value,
            "ConferenceCreatedEvent",
            $$"""{"AggregateId":"{{conferenceId.Value}}","OccurredAt":"{{DateTimeOffset.UtcNow:O}}","Name":"Original Name","Start":"{{start:O}}","End":"{{end:O}}","LocationName":"Venue","Street":"Main St 1","City":"Berlin","State":"BE","PostalCode":"10115","Country":"Germany","OrganizerId":"{{organizerId.Value}}"}""",
            DateTimeOffset.UtcNow,
            0
        );

        var renamedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            conferenceId.Value,
            "ConferenceRenamedEvent",
            $$"""{"AggregateId":"{{conferenceId.Value}}","OccurredAt":"{{DateTimeOffset.UtcNow:O}}","Name":"Updated Name"}""",
            DateTimeOffset.UtcNow,
            1
        );

        eventStore.GetAllEvents().Returns(new List<StoredEvent> { createdEvent, renamedEvent });

        var repo = new ConferenceRepository(eventStore);

        // Act
        var result = await repo.GetAll();

        // Assert
        Assert.Single(result);
        Assert.Equal("Updated Name", result[0].Name.Value);
    }
}
