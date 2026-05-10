using System.Text.Json;
using ConferenceExample.Conference.Persistence.EventHandlers;
using ConferenceExample.Conference.Persistence.ReadModels;
using ConferenceExample.EventStore;
using NSubstitute;

namespace ConferenceExample.Conference.Persistence.UnitTests;

public class ConferenceEventHandlerTests
{
    [Fact]
    public async Task HandleConferenceCreated_CreatesReadModel()
    {
        var repository = Substitute.For<IConferenceDocumentRepository>();
        var handler = new ConferenceEventHandler(repository);

        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;
        var organizerId = Guid.CreateVersion7();

        var payload = new
        {
            Name = "Test Conference",
            Start = occurredAt.AddDays(30),
            End = occurredAt.AddDays(32),
            LocationName = "Convention Center",
            Street = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            Country = "USA",
            OrganizerId = organizerId,
            Status = "Draft",
        };

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            aggregateId,
            "ConferenceCreatedEvent",
            JsonSerializer.Serialize(payload),
            occurredAt,
            0
        );

        await handler.HandleConferenceCreated(storedEvent);

        await repository
            .Received(1)
            .Save(
                Arg.Is<ConferenceDocument>(rm =>
                    rm.Id == aggregateId.ToString()
                    && rm.Name == "Test Conference"
                    && rm.Status == "Draft"
                    && rm.Version == 0
                )
            );
    }

    [Fact]
    public async Task HandleConferenceRenamed_UpdatesNameOnly()
    {
        var repository = Substitute.For<IConferenceDocumentRepository>();
        var handler = new ConferenceEventHandler(repository);

        var aggregateId = Guid.CreateVersion7();
        var existing = new ConferenceDocument
        {
            Id = aggregateId.ToString(),
            Name = "Old Name",
            Status = "Draft",
            LocationName = "Convention Center",
            Version = 0,
        };
        repository.GetById(aggregateId).Returns(existing);

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            aggregateId,
            "ConferenceRenamedEvent",
            JsonSerializer.Serialize(new { Name = "New Name" }),
            DateTimeOffset.UtcNow,
            1
        );

        await handler.HandleConferenceRenamed(storedEvent);

        await repository
            .Received(1)
            .Update(
                Arg.Is<ConferenceDocument>(rm =>
                    rm.Name == "New Name"
                    && rm.Status == "Draft"
                    && rm.LocationName == "Convention Center"
                    && rm.Version == 1
                )
            );
    }

    [Fact]
    public async Task HandleConferenceStatusChanged_UpdatesStatusOnly()
    {
        var repository = Substitute.For<IConferenceDocumentRepository>();
        var handler = new ConferenceEventHandler(repository);

        var aggregateId = Guid.CreateVersion7();
        var existing = new ConferenceDocument
        {
            Id = aggregateId.ToString(),
            Name = "Conference",
            Status = "Draft",
            Version = 0,
        };
        repository.GetById(aggregateId).Returns(existing);

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            aggregateId,
            "ConferenceStatusChangedEvent",
            JsonSerializer.Serialize(new { Status = "CallForSpeakers" }),
            DateTimeOffset.UtcNow,
            1
        );

        await handler.HandleConferenceStatusChanged(storedEvent);

        await repository
            .Received(1)
            .Update(
                Arg.Is<ConferenceDocument>(rm =>
                    rm.Name == "Conference"
                    && rm.Status == "CallForSpeakers"
                    && rm.Version == 1
                )
            );
    }

    [Fact]
    public async Task HandleConferenceDetailsUpdated_UpdatesNameTimeAndLocation()
    {
        var repository = Substitute.For<IConferenceDocumentRepository>();
        var handler = new ConferenceEventHandler(repository);

        var aggregateId = Guid.CreateVersion7();
        var existing = new ConferenceDocument
        {
            Id = aggregateId.ToString(),
            Name = "Old",
            LocationName = "Old Loc",
            Status = "Draft",
            Version = 0,
        };
        repository.GetById(aggregateId).Returns(existing);

        var occurredAt = DateTimeOffset.UtcNow;
        var payload = new
        {
            Name = "New Name",
            Start = occurredAt.AddDays(30),
            End = occurredAt.AddDays(32),
            LocationName = "Convention Center",
            Street = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            Country = "USA",
        };

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            aggregateId,
            "ConferenceDetailsUpdatedEvent",
            JsonSerializer.Serialize(payload),
            occurredAt,
            1
        );

        await handler.HandleConferenceDetailsUpdated(storedEvent);

        await repository
            .Received(1)
            .Update(
                Arg.Is<ConferenceDocument>(rm =>
                    rm.Name == "New Name"
                    && rm.LocationName == "Convention Center"
                    && rm.Street == "123 Main St"
                    && rm.City == "Springfield"
                    && rm.Status == "Draft"
                    && rm.Version == 1
                )
            );
    }

    [Fact]
    public async Task HandleTalkTypeDefined_AddsTalkType()
    {
        var repository = Substitute.For<IConferenceDocumentRepository>();
        var handler = new ConferenceEventHandler(repository);

        var aggregateId = Guid.CreateVersion7();
        var talkTypeId = Guid.CreateVersion7();
        var existing = new ConferenceDocument
        {
            Id = aggregateId.ToString(),
            Name = "Conference",
            Status = "Draft",
            Version = 0,
        };
        repository.GetById(aggregateId).Returns(existing);

        var payload = new
        {
            TalkTypeId = talkTypeId,
            TalkTypeName = "Keynote",
            TalkTypeDurationInMinutes = 60,
        };

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            aggregateId,
            "TalkTypeDefinedEvent",
            JsonSerializer.Serialize(payload),
            DateTimeOffset.UtcNow,
            1
        );

        await handler.HandleTalkTypeDefined(storedEvent);

        await repository
            .Received(1)
            .Update(
                Arg.Is<ConferenceDocument>(rm =>
                    rm.TalkTypes.Count == 1
                    && rm.TalkTypes[0].Id == talkTypeId.ToString()
                    && rm.TalkTypes[0].DurationInMinutes == 60
                    && rm.Version == 1
                )
            );
    }

    [Fact]
    public async Task HandleTalkTypeRemoved_RemovesTalkType()
    {
        var repository = Substitute.For<IConferenceDocumentRepository>();
        var handler = new ConferenceEventHandler(repository);

        var aggregateId = Guid.CreateVersion7();
        var talkTypeId = Guid.CreateVersion7();
        var existing = new ConferenceDocument
        {
            Id = aggregateId.ToString(),
            Name = "Conference",
            Status = "Draft",
            Version = 0,
            TalkTypes =
            [
                new() { Id = talkTypeId.ToString(), Name = "Workshop" },
            ],
        };
        repository.GetById(aggregateId).Returns(existing);

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            aggregateId,
            "TalkTypeRemovedEvent",
            JsonSerializer.Serialize(new { TalkTypeId = talkTypeId }),
            DateTimeOffset.UtcNow,
            1
        );

        await handler.HandleTalkTypeRemoved(storedEvent);

        await repository
            .Received(1)
            .Update(
                Arg.Is<ConferenceDocument>(rm => rm.TalkTypes.Count == 0 && rm.Version == 1)
            );
    }

    [Fact]
    public async Task HandleConferenceRenamed_MissingReadModel_DoesNothing()
    {
        var repository = Substitute.For<IConferenceDocumentRepository>();
        var handler = new ConferenceEventHandler(repository);

        var aggregateId = Guid.CreateVersion7();
        repository.GetById(aggregateId).Returns((ConferenceDocument?)null);

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            aggregateId,
            "ConferenceRenamedEvent",
            JsonSerializer.Serialize(new { Name = "Whatever" }),
            DateTimeOffset.UtcNow,
            1
        );

        await handler.HandleConferenceRenamed(storedEvent);

        await repository.DidNotReceive().Update(Arg.Any<ConferenceDocument>());
    }
}
