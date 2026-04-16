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
}
