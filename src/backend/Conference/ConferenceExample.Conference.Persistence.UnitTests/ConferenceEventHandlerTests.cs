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
        var repository = Substitute.For<IConferenceReadModelRepository>();
        var handler = new ConferenceEventHandler(repository);

        var aggregateId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;
        var organizerId = Guid.NewGuid();

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
            Guid.NewGuid(),
            aggregateId,
            "ConferenceCreatedEvent",
            JsonSerializer.Serialize(payload),
            occurredAt,
            1
        );

        repository.GetById(aggregateId).Returns((ConferenceReadModel?)null);

        // Act
        await handler.HandleConferenceDomainEvent(storedEvent);

        // Assert
        await repository
            .Received(1)
            .Save(
                Arg.Is<ConferenceReadModel>(rm =>
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
        var repository = Substitute.For<IConferenceReadModelRepository>();
        var handler = new ConferenceEventHandler(repository);

        var aggregateId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;
        var organizerId = Guid.NewGuid();

        var existingReadModel = new ConferenceReadModel
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
            Guid.NewGuid(),
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
                Arg.Is<ConferenceReadModel>(rm =>
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
        var repository = Substitute.For<IConferenceReadModelRepository>();
        var handler = new ConferenceEventHandler(repository);

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
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
        var repository = Substitute.For<IConferenceReadModelRepository>();
        var handler = new ConferenceEventHandler(repository);

        var aggregateId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();

        var existingReadModel = new ConferenceReadModel
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
            Guid.NewGuid(),
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
        await repository.DidNotReceive().Update(Arg.Any<ConferenceReadModel>());
        Assert.Equal("Existing Name", existingReadModel.Name); // Name unchanged
    }

    [Fact]
    public async Task HandleConferenceDomainEvent_OutOfOrderEvent_DoesNotUpdate()
    {
        // Arrange
        var repository = Substitute.For<IConferenceReadModelRepository>();
        var handler = new ConferenceEventHandler(repository);

        var aggregateId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();

        var existingReadModel = new ConferenceReadModel
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
            Guid.NewGuid(),
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
        await repository.DidNotReceive().Update(Arg.Any<ConferenceReadModel>());
        Assert.Equal("Current Name", existingReadModel.Name); // Name unchanged
        Assert.Equal(3, existingReadModel.Version); // Version unchanged
    }

    [Fact]
    public async Task HandleTalkTypeDefined_DuplicateEvent_DoesNotUpdate()
    {
        // Arrange
        var repository = Substitute.For<IConferenceReadModelRepository>();
        var handler = new ConferenceEventHandler(repository);

        var aggregateId = Guid.NewGuid();
        var talkTypeId = Guid.NewGuid();

        var existingReadModel = new ConferenceReadModel
        {
            Id = aggregateId.ToString(),
            Name = "Conference",
            Version = 2, // Already at version 2
            TalkTypes = new List<ConferenceReadModel.TalkTypeReadModel>
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
            OrganizerId = Guid.NewGuid(),
            Status = "Draft",
            TalkTypeId = talkTypeId,
            TalkTypeName = "Keynote",
        };

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
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
        await repository.DidNotReceive().Update(Arg.Any<ConferenceReadModel>());
        Assert.Single(existingReadModel.TalkTypes); // TalkTypes unchanged
    }

    [Fact]
    public async Task HandleTalkTypeRemoved_OutOfOrderEvent_DoesNotUpdate()
    {
        // Arrange
        var repository = Substitute.For<IConferenceReadModelRepository>();
        var handler = new ConferenceEventHandler(repository);

        var aggregateId = Guid.NewGuid();
        var talkTypeId = Guid.NewGuid();

        var existingReadModel = new ConferenceReadModel
        {
            Id = aggregateId.ToString(),
            Name = "Conference",
            Version = 3, // Already at version 3
            TalkTypes = new List<ConferenceReadModel.TalkTypeReadModel>
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
            OrganizerId = Guid.NewGuid(),
            Status = "Draft",
            TalkTypeId = talkTypeId,
        };

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
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
        await repository.DidNotReceive().Update(Arg.Any<ConferenceReadModel>());
        Assert.Single(existingReadModel.TalkTypes); // TalkTypes unchanged
        Assert.Equal(3, existingReadModel.Version); // Version unchanged
    }
}
