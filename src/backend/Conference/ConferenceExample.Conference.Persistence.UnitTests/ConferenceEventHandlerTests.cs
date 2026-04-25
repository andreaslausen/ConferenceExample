using System.Text.Json;
using ConferenceExample.Conference.Persistence.EventHandlers;
using ConferenceExample.Conference.Persistence.ReadModels;
using ConferenceExample.EventStore;
using NSubstitute;

namespace ConferenceExample.Conference.Persistence.UnitTests;

public class ConferenceEventHandlerTests
{
    [Fact]
    public async Task HandleConferenceDomainEvent_NewConference_CreatesReadModel()
    {
        // Arrange
        var repository = Substitute.For<IConferenceDocumentRepository>();
        var handler = new ConferenceEventHandler(repository);

        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;
        var organizerId = Guid.CreateVersion7();

        var payload = new
        {
            AggregateId = aggregateId,
            OccurredAt = occurredAt,
            Version = 1L,
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
            1
        );

        repository.GetById(aggregateId).Returns((ConferenceDocument?)null);

        // Act
        await handler.HandleConferenceDomainEvent(storedEvent);

        // Assert
        await repository
            .Received(1)
            .Save(
                Arg.Is<ConferenceDocument>(rm =>
                    rm.Id == aggregateId.ToString()
                    && rm.Name == "Test Conference"
                    && rm.Status == "Draft"
                    && rm.Version == 1
                )
            );
    }

    [Fact]
    public async Task HandleConferenceDomainEvent_ExistingConference_UpdatesReadModel()
    {
        // Arrange
        var repository = Substitute.For<IConferenceDocumentRepository>();
        var handler = new ConferenceEventHandler(repository);

        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;
        var organizerId = Guid.CreateVersion7();

        var existingReadModel = new ConferenceDocument
        {
            Id = aggregateId.ToString(),
            Name = "Old Name",
            Version = 1,
        };

        var payload = new
        {
            AggregateId = aggregateId,
            OccurredAt = occurredAt,
            Version = 2L,
            Name = "New Name",
            Start = occurredAt.AddDays(30),
            End = occurredAt.AddDays(32),
            LocationName = "Convention Center",
            Street = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            Country = "USA",
            OrganizerId = organizerId,
            Status = "Published",
        };

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            aggregateId,
            "ConferenceRenamedEvent",
            JsonSerializer.Serialize(payload),
            occurredAt,
            2
        );

        repository.GetById(aggregateId).Returns(existingReadModel);

        // Act
        await handler.HandleConferenceDomainEvent(storedEvent);

        // Assert
        await repository
            .Received(1)
            .Update(
                Arg.Is<ConferenceDocument>(rm =>
                    rm.Id == aggregateId.ToString()
                    && rm.Name == "New Name"
                    && rm.Status == "Published"
                    && rm.Version == 2
                )
            );
    }

    [Fact]
    public async Task HandleConferenceDomainEvent_InvalidPayload_ThrowsException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceDocumentRepository>();
        var handler = new ConferenceEventHandler(repository);

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "ConferenceCreatedEvent",
            "invalid json",
            DateTimeOffset.UtcNow,
            1
        );

        // Act & Assert
        await Assert.ThrowsAsync<System.Text.Json.JsonException>(async () =>
            await handler.HandleConferenceDomainEvent(storedEvent)
        );
    }

    // Idempotency Tests
    [Fact]
    public async Task HandleConferenceDomainEvent_DuplicateEvent_DoesNotUpdate()
    {
        // Arrange
        var repository = Substitute.For<IConferenceDocumentRepository>();
        var handler = new ConferenceEventHandler(repository);

        var aggregateId = Guid.CreateVersion7();
        var organizerId = Guid.CreateVersion7();

        var existingReadModel = new ConferenceDocument
        {
            Id = aggregateId.ToString(),
            Name = "Existing Name",
            Status = "Published",
            Version = 2, // Already at version 2
        };

        var payload = new
        {
            AggregateId = aggregateId,
            OccurredAt = DateTimeOffset.UtcNow,
            Version = 2L, // Same version - duplicate
            Name = "Duplicate Name",
            Start = DateTimeOffset.UtcNow.AddDays(30),
            End = DateTimeOffset.UtcNow.AddDays(32),
            LocationName = "Convention Center",
            Street = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            Country = "USA",
            OrganizerId = organizerId,
            Status = "Published",
        };

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            aggregateId,
            "ConferenceRenamedEvent",
            JsonSerializer.Serialize(payload),
            DateTimeOffset.UtcNow,
            2
        );

        repository.GetById(aggregateId).Returns(existingReadModel);

        // Act
        await handler.HandleConferenceDomainEvent(storedEvent);

        // Assert - should not update
        await repository.DidNotReceive().Update(Arg.Any<ConferenceDocument>());
        Assert.Equal("Existing Name", existingReadModel.Name); // Name unchanged
    }

    [Fact]
    public async Task HandleConferenceDomainEvent_OutOfOrderEvent_DoesNotUpdate()
    {
        // Arrange
        var repository = Substitute.For<IConferenceDocumentRepository>();
        var handler = new ConferenceEventHandler(repository);

        var aggregateId = Guid.CreateVersion7();
        var organizerId = Guid.CreateVersion7();

        var existingReadModel = new ConferenceDocument
        {
            Id = aggregateId.ToString(),
            Name = "Current Name",
            Status = "Published",
            Version = 3, // Already at version 3
        };

        var payload = new
        {
            AggregateId = aggregateId,
            OccurredAt = DateTimeOffset.UtcNow,
            Version = 2L, // Older version - out of order
            Name = "Old Name",
            Start = DateTimeOffset.UtcNow.AddDays(30),
            End = DateTimeOffset.UtcNow.AddDays(32),
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
            "ConferenceStatusChangedEvent",
            JsonSerializer.Serialize(payload),
            DateTimeOffset.UtcNow,
            2
        );

        repository.GetById(aggregateId).Returns(existingReadModel);

        // Act
        await handler.HandleConferenceDomainEvent(storedEvent);

        // Assert - should not update
        await repository.DidNotReceive().Update(Arg.Any<ConferenceDocument>());
        Assert.Equal("Current Name", existingReadModel.Name); // Name unchanged
        Assert.Equal(3, existingReadModel.Version); // Version unchanged
    }

    [Fact]
    public async Task HandleRoomAdded_UpdatesAllConferenceFields()
    {
        // Arrange
        var repository = Substitute.For<IConferenceDocumentRepository>();
        var handler = new ConferenceEventHandler(repository);

        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;
        var organizerId = Guid.CreateVersion7();

        var existingReadModel = new ConferenceDocument
        {
            Id = aggregateId.ToString(),
            Name = "Old Name",
            Start = occurredAt,
            End = occurredAt,
            LocationName = "Old Location",
            Street = "Old Street",
            City = "Old City",
            State = "Old State",
            PostalCode = "00000",
            Country = "Old Country",
            OrganizerId = Guid.CreateVersion7().ToString(),
            Status = "Draft",
            Version = 1,
        };

        var payload = new
        {
            AggregateId = aggregateId,
            OccurredAt = occurredAt,
            Version = 2L,
            Name = "New Name",
            Start = occurredAt.AddDays(30),
            End = occurredAt.AddDays(32),
            LocationName = "Convention Center",
            Street = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            Country = "USA",
            OrganizerId = organizerId,
            Status = "Published",
            RoomId = Guid.CreateVersion7(),
            RoomName = "Main Hall",
        };

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            aggregateId,
            "RoomAddedEvent",
            JsonSerializer.Serialize(payload),
            occurredAt,
            2
        );

        repository.GetById(aggregateId).Returns(existingReadModel);

        // Act
        await handler.HandleRoomAdded(storedEvent);

        // Assert
        await repository
            .Received(1)
            .Update(
                Arg.Is<ConferenceDocument>(rm =>
                    rm.Name == "New Name"
                    && rm.LocationName == "Convention Center"
                    && rm.Street == "123 Main St"
                    && rm.City == "Springfield"
                    && rm.State == "IL"
                    && rm.PostalCode == "62701"
                    && rm.Country == "USA"
                    && rm.OrganizerId == organizerId.ToString()
                    && rm.Status == "Published"
                    && rm.Version == 2
                )
            );
    }

    [Fact]
    public async Task HandleRoomRemoved_UpdatesAllConferenceFields()
    {
        // Arrange
        var repository = Substitute.For<IConferenceDocumentRepository>();
        var handler = new ConferenceEventHandler(repository);

        var aggregateId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;
        var organizerId = Guid.CreateVersion7();

        var existingReadModel = new ConferenceDocument
        {
            Id = aggregateId.ToString(),
            Name = "Old Name",
            Start = occurredAt,
            End = occurredAt,
            LocationName = "Old Location",
            Street = "Old Street",
            City = "Old City",
            State = "Old State",
            PostalCode = "00000",
            Country = "Old Country",
            OrganizerId = Guid.CreateVersion7().ToString(),
            Status = "Draft",
            Version = 1,
        };

        var payload = new
        {
            AggregateId = aggregateId,
            OccurredAt = occurredAt,
            Version = 2L,
            Name = "New Name",
            Start = occurredAt.AddDays(30),
            End = occurredAt.AddDays(32),
            LocationName = "Convention Center",
            Street = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            Country = "USA",
            OrganizerId = organizerId,
            Status = "Published",
            RoomId = Guid.CreateVersion7(),
        };

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            aggregateId,
            "RoomRemovedEvent",
            JsonSerializer.Serialize(payload),
            occurredAt,
            2
        );

        repository.GetById(aggregateId).Returns(existingReadModel);

        // Act
        await handler.HandleRoomRemoved(storedEvent);

        // Assert
        await repository
            .Received(1)
            .Update(
                Arg.Is<ConferenceDocument>(rm =>
                    rm.Name == "New Name"
                    && rm.LocationName == "Convention Center"
                    && rm.Street == "123 Main St"
                    && rm.City == "Springfield"
                    && rm.State == "IL"
                    && rm.PostalCode == "62701"
                    && rm.Country == "USA"
                    && rm.OrganizerId == organizerId.ToString()
                    && rm.Status == "Published"
                    && rm.Version == 2
                )
            );
    }

    [Fact]
    public async Task HandleTalkTypeDefined_DuplicateEvent_DoesNotUpdate()
    {
        // Arrange
        var repository = Substitute.For<IConferenceDocumentRepository>();
        var handler = new ConferenceEventHandler(repository);

        var aggregateId = Guid.CreateVersion7();
        var talkTypeId = Guid.CreateVersion7();

        var existingReadModel = new ConferenceDocument
        {
            Id = aggregateId.ToString(),
            Name = "Conference",
            Version = 2, // Already at version 2
            TalkTypes = new List<ConferenceDocument.TalkTypeDocument>
            {
                new() { Id = talkTypeId.ToString(), Name = "Keynote" },
            },
        };

        var payload = new
        {
            AggregateId = aggregateId,
            OccurredAt = DateTimeOffset.UtcNow,
            Version = 2L, // Same version - duplicate
            Name = "Conference",
            Start = DateTimeOffset.UtcNow.AddDays(30),
            End = DateTimeOffset.UtcNow.AddDays(32),
            LocationName = "Convention Center",
            Street = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            Country = "USA",
            OrganizerId = Guid.CreateVersion7(),
            Status = "Draft",
            TalkTypeId = talkTypeId,
            TalkTypeName = "Keynote",
        };

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            aggregateId,
            "TalkTypeDefinedEvent",
            JsonSerializer.Serialize(payload),
            DateTimeOffset.UtcNow,
            2
        );

        repository.GetById(aggregateId).Returns(existingReadModel);

        // Act
        await handler.HandleTalkTypeDefined(storedEvent);

        // Assert - should not update
        await repository.DidNotReceive().Update(Arg.Any<ConferenceDocument>());
        Assert.Single(existingReadModel.TalkTypes); // TalkTypes unchanged
    }

    [Fact]
    public async Task HandleTalkTypeRemoved_OutOfOrderEvent_DoesNotUpdate()
    {
        // Arrange
        var repository = Substitute.For<IConferenceDocumentRepository>();
        var handler = new ConferenceEventHandler(repository);

        var aggregateId = Guid.CreateVersion7();
        var talkTypeId = Guid.CreateVersion7();

        var existingReadModel = new ConferenceDocument
        {
            Id = aggregateId.ToString(),
            Name = "Conference",
            Version = 3, // Already at version 3
            TalkTypes = new List<ConferenceDocument.TalkTypeDocument>
            {
                new() { Id = talkTypeId.ToString(), Name = "Workshop" },
            },
        };

        var payload = new
        {
            AggregateId = aggregateId,
            OccurredAt = DateTimeOffset.UtcNow,
            Version = 2L, // Older version - out of order
            Name = "Conference",
            Start = DateTimeOffset.UtcNow.AddDays(30),
            End = DateTimeOffset.UtcNow.AddDays(32),
            LocationName = "Convention Center",
            Street = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            Country = "USA",
            OrganizerId = Guid.CreateVersion7(),
            Status = "Draft",
            TalkTypeId = talkTypeId,
        };

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            aggregateId,
            "TalkTypeRemovedEvent",
            JsonSerializer.Serialize(payload),
            DateTimeOffset.UtcNow,
            2
        );

        repository.GetById(aggregateId).Returns(existingReadModel);

        // Act
        await handler.HandleTalkTypeRemoved(storedEvent);

        // Assert - should not update
        await repository.DidNotReceive().Update(Arg.Any<ConferenceDocument>());
        Assert.Single(existingReadModel.TalkTypes); // TalkTypes unchanged
        Assert.Equal(3, existingReadModel.Version); // Version unchanged
    }
}
