using System.Text.Json;
using ConferenceExample.Conference.Persistence.EventHandlers;
using ConferenceExample.Conference.Persistence.ReadModels;
using ConferenceExample.EventStore;
using NSubstitute;

namespace ConferenceExample.Conference.Persistence.UnitTests;

public class TalkEventHandlerTests
{
    [Fact]
    public async Task HandleTalkDomainEvent_NewTalk_CreatesReadModel()
    {
        // Arrange
        var repository = Substitute.For<IConferenceTalkReadModelRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var speakerId = Guid.NewGuid();
        var talkTypeId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        var payload = new
        {
            AggregateId = talkId,
            OccurredAt = occurredAt,
            Version = 1L,
            Title = "Test Talk",
            Abstract = "Test Abstract",
            SpeakerId = speakerId,
            Tags = new List<string> { "dotnet", "testing" },
            TalkTypeId = talkTypeId,
            ConferenceId = conferenceId,
            Status = "Submitted",
        };

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            talkId,
            "TalkSubmittedEvent",
            JsonSerializer.Serialize(payload),
            occurredAt,
            1
        );

        repository.GetById(talkId).Returns((ConferenceTalkReadModel?)null);

        // Act
        await handler.HandleTalkDomainEvent(storedEvent);

        // Assert
        await repository
            .Received(1)
            .Save(
                Arg.Is<ConferenceTalkReadModel>(rm =>
                    rm.Id == talkId.ToString()
                    && rm.Title == "Test Talk"
                    && rm.ConferenceId == conferenceId.ToString()
                    && rm.Status == "Submitted"
                    && rm.Version == 1
                )
            );
    }

    [Fact]
    public async Task HandleTalkAccepted_UpdatesReadModelStatus()
    {
        // Arrange
        var repository = Substitute.For<IConferenceTalkReadModelRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        var existingReadModel = new ConferenceTalkReadModel
        {
            Id = talkId.ToString(),
            Status = "Submitted",
            Version = 1,
        };

        var payload = new
        {
            AggregateId = Guid.NewGuid(),
            OccurredAt = occurredAt,
            Version = 2L,
            Name = "Conference",
            Start = occurredAt,
            End = occurredAt,
            LocationName = "Location",
            Street = "Street",
            City = "City",
            State = "State",
            PostalCode = "12345",
            Country = "Country",
            OrganizerId = Guid.NewGuid(),
            Status = "Draft",
            TalkId = talkId,
        };

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TalkAcceptedEvent",
            JsonSerializer.Serialize(payload),
            occurredAt,
            2
        );

        repository.GetById(talkId).Returns(existingReadModel);

        // Act
        await handler.HandleTalkAccepted(storedEvent);

        // Assert
        await repository
            .Received(1)
            .Update(
                Arg.Is<ConferenceTalkReadModel>(rm =>
                    rm.Id == talkId.ToString() && rm.Status == "Accepted"
                )
            );
    }

    [Fact]
    public async Task HandleTalkScheduled_UpdatesSlotTimes()
    {
        // Arrange
        var repository = Substitute.For<IConferenceTalkReadModelRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.NewGuid();
        var talkStart = DateTimeOffset.UtcNow.AddDays(30);
        var talkEnd = talkStart.AddHours(1);

        var existingReadModel = new ConferenceTalkReadModel
        {
            Id = talkId.ToString(),
            Status = "Accepted",
            Version = 1,
        };

        var payload = new
        {
            AggregateId = Guid.NewGuid(),
            OccurredAt = DateTimeOffset.UtcNow,
            Version = 2L,
            Name = "Conference",
            ConferenceStart = DateTimeOffset.UtcNow.AddDays(30),
            ConferenceEnd = DateTimeOffset.UtcNow.AddDays(32),
            LocationName = "Location",
            Street = "Street",
            City = "City",
            State = "State",
            PostalCode = "12345",
            Country = "Country",
            OrganizerId = Guid.NewGuid(),
            Status = "Draft",
            TalkId = talkId,
            TalkStart = talkStart,
            TalkEnd = talkEnd,
        };

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TalkScheduledEvent",
            JsonSerializer.Serialize(payload),
            DateTimeOffset.UtcNow,
            2
        );

        repository.GetById(talkId).Returns(existingReadModel);

        // Act
        await handler.HandleTalkScheduled(storedEvent);

        // Assert
        await repository
            .Received(1)
            .Update(
                Arg.Is<ConferenceTalkReadModel>(rm =>
                    rm.Id == talkId.ToString() && rm.SlotStart == talkStart && rm.SlotEnd == talkEnd
                )
            );
    }

    [Fact]
    public async Task HandleTalkRejected_UpdatesReadModelStatus()
    {
        // Arrange
        var repository = Substitute.For<IConferenceTalkReadModelRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        var existingReadModel = new ConferenceTalkReadModel
        {
            Id = talkId.ToString(),
            Status = "Submitted",
            Version = 1,
        };

        var payload = new
        {
            AggregateId = Guid.NewGuid(),
            OccurredAt = occurredAt,
            Version = 2L,
            Name = "Conference",
            Start = occurredAt,
            End = occurredAt,
            LocationName = "Location",
            Street = "Street",
            City = "City",
            State = "State",
            PostalCode = "12345",
            Country = "Country",
            OrganizerId = Guid.NewGuid(),
            Status = "Draft",
            TalkId = talkId,
        };

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TalkRejectedEvent",
            JsonSerializer.Serialize(payload),
            occurredAt,
            2
        );

        repository.GetById(talkId).Returns(existingReadModel);

        // Act
        await handler.HandleTalkRejected(storedEvent);

        // Assert
        await repository
            .Received(1)
            .Update(
                Arg.Is<ConferenceTalkReadModel>(rm =>
                    rm.Id == talkId.ToString() && rm.Status == "Rejected"
                )
            );
    }

    [Fact]
    public async Task HandleTalkRejected_WithNullPayload_DoesNotUpdate()
    {
        // Arrange
        var repository = Substitute.For<IConferenceTalkReadModelRepository>();
        var handler = new TalkEventHandler(repository);

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TalkRejectedEvent",
            "null",
            DateTimeOffset.UtcNow,
            1
        );

        // Act
        await handler.HandleTalkRejected(storedEvent);

        // Assert
        await repository.DidNotReceive().GetById(Arg.Any<Guid>());
        await repository.DidNotReceive().Update(Arg.Any<ConferenceTalkReadModel>());
    }

    [Fact]
    public async Task HandleTalkRejected_WithNonExistingReadModel_DoesNotUpdate()
    {
        // Arrange
        var repository = Substitute.For<IConferenceTalkReadModelRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.NewGuid();

        var payload = new
        {
            AggregateId = Guid.NewGuid(),
            OccurredAt = DateTimeOffset.UtcNow,
            Version = 2L,
            Name = "Conference",
            Start = DateTimeOffset.UtcNow,
            End = DateTimeOffset.UtcNow,
            LocationName = "Location",
            Street = "Street",
            City = "City",
            State = "State",
            PostalCode = "12345",
            Country = "Country",
            OrganizerId = Guid.NewGuid(),
            Status = "Draft",
            TalkId = talkId,
        };

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TalkRejectedEvent",
            JsonSerializer.Serialize(payload),
            DateTimeOffset.UtcNow,
            2
        );

        repository.GetById(talkId).Returns((ConferenceTalkReadModel?)null);

        // Act
        await handler.HandleTalkRejected(storedEvent);

        // Assert
        await repository.Received(1).GetById(talkId);
        await repository.DidNotReceive().Update(Arg.Any<ConferenceTalkReadModel>());
    }

    [Fact]
    public async Task HandleTalkAssignedToRoom_UpdatesRoomInformation()
    {
        // Arrange
        var repository = Substitute.For<IConferenceTalkReadModelRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        var existingReadModel = new ConferenceTalkReadModel
        {
            Id = talkId.ToString(),
            Status = "Accepted",
            Version = 1,
        };

        var payload = new
        {
            AggregateId = Guid.NewGuid(),
            OccurredAt = occurredAt,
            Version = 2L,
            Name = "Conference",
            Start = occurredAt,
            End = occurredAt,
            LocationName = "Location",
            Street = "Street",
            City = "City",
            State = "State",
            PostalCode = "12345",
            Country = "Country",
            OrganizerId = Guid.NewGuid(),
            Status = "Draft",
            TalkId = talkId,
            RoomId = roomId,
            RoomName = "Main Hall",
        };

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TalkAssignedToRoomEvent",
            JsonSerializer.Serialize(payload),
            occurredAt,
            2
        );

        repository.GetById(talkId).Returns(existingReadModel);

        // Act
        await handler.HandleTalkAssignedToRoom(storedEvent);

        // Assert
        await repository
            .Received(1)
            .Update(
                Arg.Is<ConferenceTalkReadModel>(rm =>
                    rm.Id == talkId.ToString()
                    && rm.RoomId == roomId.ToString()
                    && rm.RoomName == "Main Hall"
                )
            );
    }

    [Fact]
    public async Task HandleTalkAssignedToRoom_WithNullPayload_DoesNotUpdate()
    {
        // Arrange
        var repository = Substitute.For<IConferenceTalkReadModelRepository>();
        var handler = new TalkEventHandler(repository);

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TalkAssignedToRoomEvent",
            "null",
            DateTimeOffset.UtcNow,
            1
        );

        // Act
        await handler.HandleTalkAssignedToRoom(storedEvent);

        // Assert
        await repository.DidNotReceive().GetById(Arg.Any<Guid>());
        await repository.DidNotReceive().Update(Arg.Any<ConferenceTalkReadModel>());
    }

    [Fact]
    public async Task HandleTalkAssignedToRoom_WithNonExistingReadModel_DoesNotUpdate()
    {
        // Arrange
        var repository = Substitute.For<IConferenceTalkReadModelRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.NewGuid();

        var payload = new
        {
            AggregateId = Guid.NewGuid(),
            OccurredAt = DateTimeOffset.UtcNow,
            Version = 2L,
            Name = "Conference",
            Start = DateTimeOffset.UtcNow,
            End = DateTimeOffset.UtcNow,
            LocationName = "Location",
            Street = "Street",
            City = "City",
            State = "State",
            PostalCode = "12345",
            Country = "Country",
            OrganizerId = Guid.NewGuid(),
            Status = "Draft",
            TalkId = talkId,
            RoomId = Guid.NewGuid(),
            RoomName = "Main Hall",
        };

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TalkAssignedToRoomEvent",
            JsonSerializer.Serialize(payload),
            DateTimeOffset.UtcNow,
            2
        );

        repository.GetById(talkId).Returns((ConferenceTalkReadModel?)null);

        // Act
        await handler.HandleTalkAssignedToRoom(storedEvent);

        // Assert
        await repository.Received(1).GetById(talkId);
        await repository.DidNotReceive().Update(Arg.Any<ConferenceTalkReadModel>());
    }

    [Fact]
    public async Task HandleTalkDomainEvent_UpdatesExistingReadModel()
    {
        // Arrange
        var repository = Substitute.For<IConferenceTalkReadModelRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var speakerId = Guid.NewGuid();
        var talkTypeId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        var existingReadModel = new ConferenceTalkReadModel
        {
            Id = talkId.ToString(),
            Title = "Old Title",
            Status = "Submitted",
            Version = 1,
        };

        var payload = new
        {
            AggregateId = talkId,
            OccurredAt = occurredAt,
            Version = 2L,
            Title = "Updated Talk",
            Abstract = "Updated Abstract",
            SpeakerId = speakerId,
            Tags = new List<string> { "csharp", "dotnet" },
            TalkTypeId = talkTypeId,
            ConferenceId = conferenceId,
            Status = "Submitted",
        };

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            talkId,
            "TalkEditedEvent",
            JsonSerializer.Serialize(payload),
            occurredAt,
            2
        );

        repository.GetById(talkId).Returns(existingReadModel);

        // Act
        await handler.HandleTalkDomainEvent(storedEvent);

        // Assert
        await repository
            .Received(1)
            .Update(
                Arg.Is<ConferenceTalkReadModel>(rm =>
                    rm.Id == talkId.ToString()
                    && rm.Title == "Updated Talk"
                    && rm.Abstract == "Updated Abstract"
                    && rm.Version == 2
                )
            );
    }

    [Fact]
    public async Task HandleTalkDomainEvent_WithNullPayload_DoesNotCreateOrUpdate()
    {
        // Arrange
        var repository = Substitute.For<IConferenceTalkReadModelRepository>();
        var handler = new TalkEventHandler(repository);

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TalkSubmittedEvent",
            "null",
            DateTimeOffset.UtcNow,
            1
        );

        // Act
        await handler.HandleTalkDomainEvent(storedEvent);

        // Assert
        await repository.DidNotReceive().Save(Arg.Any<ConferenceTalkReadModel>());
        await repository.DidNotReceive().Update(Arg.Any<ConferenceTalkReadModel>());
    }

    [Fact]
    public async Task HandleTalkAccepted_WithNullPayload_DoesNotUpdate()
    {
        // Arrange
        var repository = Substitute.For<IConferenceTalkReadModelRepository>();
        var handler = new TalkEventHandler(repository);

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TalkAcceptedEvent",
            "null",
            DateTimeOffset.UtcNow,
            1
        );

        // Act
        await handler.HandleTalkAccepted(storedEvent);

        // Assert
        await repository.DidNotReceive().GetById(Arg.Any<Guid>());
        await repository.DidNotReceive().Update(Arg.Any<ConferenceTalkReadModel>());
    }

    [Fact]
    public async Task HandleTalkAccepted_WithNonExistingReadModel_DoesNotUpdate()
    {
        // Arrange
        var repository = Substitute.For<IConferenceTalkReadModelRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.NewGuid();

        var payload = new
        {
            AggregateId = Guid.NewGuid(),
            OccurredAt = DateTimeOffset.UtcNow,
            Version = 2L,
            Name = "Conference",
            Start = DateTimeOffset.UtcNow,
            End = DateTimeOffset.UtcNow,
            LocationName = "Location",
            Street = "Street",
            City = "City",
            State = "State",
            PostalCode = "12345",
            Country = "Country",
            OrganizerId = Guid.NewGuid(),
            Status = "Draft",
            TalkId = talkId,
        };

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TalkAcceptedEvent",
            JsonSerializer.Serialize(payload),
            DateTimeOffset.UtcNow,
            2
        );

        repository.GetById(talkId).Returns((ConferenceTalkReadModel?)null);

        // Act
        await handler.HandleTalkAccepted(storedEvent);

        // Assert
        await repository.Received(1).GetById(talkId);
        await repository.DidNotReceive().Update(Arg.Any<ConferenceTalkReadModel>());
    }

    [Fact]
    public async Task HandleTalkScheduled_WithNullPayload_DoesNotUpdate()
    {
        // Arrange
        var repository = Substitute.For<IConferenceTalkReadModelRepository>();
        var handler = new TalkEventHandler(repository);

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TalkScheduledEvent",
            "null",
            DateTimeOffset.UtcNow,
            1
        );

        // Act
        await handler.HandleTalkScheduled(storedEvent);

        // Assert
        await repository.DidNotReceive().GetById(Arg.Any<Guid>());
        await repository.DidNotReceive().Update(Arg.Any<ConferenceTalkReadModel>());
    }

    [Fact]
    public async Task HandleTalkScheduled_WithNonExistingReadModel_DoesNotUpdate()
    {
        // Arrange
        var repository = Substitute.For<IConferenceTalkReadModelRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.NewGuid();
        var talkStart = DateTimeOffset.UtcNow.AddDays(30);
        var talkEnd = talkStart.AddHours(1);

        var payload = new
        {
            AggregateId = Guid.NewGuid(),
            OccurredAt = DateTimeOffset.UtcNow,
            Version = 2L,
            Name = "Conference",
            ConferenceStart = DateTimeOffset.UtcNow.AddDays(30),
            ConferenceEnd = DateTimeOffset.UtcNow.AddDays(32),
            LocationName = "Location",
            Street = "Street",
            City = "City",
            State = "State",
            PostalCode = "12345",
            Country = "Country",
            OrganizerId = Guid.NewGuid(),
            Status = "Draft",
            TalkId = talkId,
            TalkStart = talkStart,
            TalkEnd = talkEnd,
        };

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TalkScheduledEvent",
            JsonSerializer.Serialize(payload),
            DateTimeOffset.UtcNow,
            2
        );

        repository.GetById(talkId).Returns((ConferenceTalkReadModel?)null);

        // Act
        await handler.HandleTalkScheduled(storedEvent);

        // Assert
        await repository.Received(1).GetById(talkId);
        await repository.DidNotReceive().Update(Arg.Any<ConferenceTalkReadModel>());
    }

    // Idempotency Tests
    [Fact]
    public async Task HandleTalkDomainEvent_DuplicateEvent_DoesNotUpdate()
    {
        // Arrange
        var repository = Substitute.For<IConferenceTalkReadModelRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var speakerId = Guid.NewGuid();
        var talkTypeId = Guid.NewGuid();

        var existingReadModel = new ConferenceTalkReadModel
        {
            Id = talkId.ToString(),
            ConferenceId = conferenceId.ToString(),
            Title = "Existing Title",
            Version = 2, // Already at version 2
        };

        var payload = new
        {
            AggregateId = talkId,
            OccurredAt = DateTimeOffset.UtcNow,
            Version = 2L, // Same version - duplicate
            Title = "Duplicate Title",
            Abstract = "Duplicate Abstract",
            SpeakerId = speakerId,
            Tags = new List<string>(),
            TalkTypeId = talkTypeId,
            ConferenceId = conferenceId,
            Status = "Submitted",
        };

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            talkId,
            "TalkEditedEvent",
            JsonSerializer.Serialize(payload),
            DateTimeOffset.UtcNow,
            2
        );

        repository.GetById(talkId).Returns(existingReadModel);

        // Act
        await handler.HandleTalkDomainEvent(storedEvent);

        // Assert - should not update
        await repository.DidNotReceive().Update(Arg.Any<ConferenceTalkReadModel>());
    }

    [Fact]
    public async Task HandleTalkAccepted_DuplicateEvent_DoesNotUpdate()
    {
        // Arrange
        var repository = Substitute.For<IConferenceTalkReadModelRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.NewGuid();

        var existingReadModel = new ConferenceTalkReadModel
        {
            Id = talkId.ToString(),
            Status = "Accepted",
            Version = 2, // Already at version 2
        };

        var payload = new
        {
            AggregateId = Guid.NewGuid(),
            OccurredAt = DateTimeOffset.UtcNow,
            Version = 2L, // Same version - duplicate
            Name = "Conference",
            Start = DateTimeOffset.UtcNow,
            End = DateTimeOffset.UtcNow,
            LocationName = "Location",
            Street = "Street",
            City = "City",
            State = "State",
            PostalCode = "12345",
            Country = "Country",
            OrganizerId = Guid.NewGuid(),
            Status = "Draft",
            TalkId = talkId,
        };

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TalkAcceptedEvent",
            JsonSerializer.Serialize(payload),
            DateTimeOffset.UtcNow,
            2
        );

        repository.GetById(talkId).Returns(existingReadModel);

        // Act
        await handler.HandleTalkAccepted(storedEvent);

        // Assert - should not update
        await repository.DidNotReceive().Update(Arg.Any<ConferenceTalkReadModel>());
    }

    [Fact]
    public async Task HandleTalkRejected_OutOfOrderEvent_DoesNotUpdate()
    {
        // Arrange
        var repository = Substitute.For<IConferenceTalkReadModelRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.NewGuid();

        var existingReadModel = new ConferenceTalkReadModel
        {
            Id = talkId.ToString(),
            Status = "Rejected",
            Version = 3, // Already at version 3
        };

        var payload = new
        {
            AggregateId = Guid.NewGuid(),
            OccurredAt = DateTimeOffset.UtcNow,
            Version = 2L, // Older version - out of order
            Name = "Conference",
            Start = DateTimeOffset.UtcNow,
            End = DateTimeOffset.UtcNow,
            LocationName = "Location",
            Street = "Street",
            City = "City",
            State = "State",
            PostalCode = "12345",
            Country = "Country",
            OrganizerId = Guid.NewGuid(),
            Status = "Draft",
            TalkId = talkId,
        };

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TalkRejectedEvent",
            JsonSerializer.Serialize(payload),
            DateTimeOffset.UtcNow,
            2
        );

        repository.GetById(talkId).Returns(existingReadModel);

        // Act
        await handler.HandleTalkRejected(storedEvent);

        // Assert - should not update
        await repository.DidNotReceive().Update(Arg.Any<ConferenceTalkReadModel>());
        Assert.Equal(3, existingReadModel.Version); // Version unchanged
    }

    [Fact]
    public async Task HandleTalkScheduled_DuplicateEvent_DoesNotUpdate()
    {
        // Arrange
        var repository = Substitute.For<IConferenceTalkReadModelRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.NewGuid();

        var existingReadModel = new ConferenceTalkReadModel
        {
            Id = talkId.ToString(),
            SlotStart = DateTimeOffset.UtcNow.AddDays(30),
            SlotEnd = DateTimeOffset.UtcNow.AddDays(30).AddHours(1),
            Version = 2, // Already at version 2
        };

        var payload = new
        {
            AggregateId = Guid.NewGuid(),
            OccurredAt = DateTimeOffset.UtcNow,
            Version = 2L, // Same version - duplicate
            Name = "Conference",
            ConferenceStart = DateTimeOffset.UtcNow.AddDays(30),
            ConferenceEnd = DateTimeOffset.UtcNow.AddDays(32),
            LocationName = "Location",
            Street = "Street",
            City = "City",
            State = "State",
            PostalCode = "12345",
            Country = "Country",
            OrganizerId = Guid.NewGuid(),
            Status = "Published",
            TalkId = talkId,
            TalkStart = DateTimeOffset.UtcNow.AddDays(30),
            TalkEnd = DateTimeOffset.UtcNow.AddDays(30).AddHours(1),
        };

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TalkScheduledEvent",
            JsonSerializer.Serialize(payload),
            DateTimeOffset.UtcNow,
            2
        );

        repository.GetById(talkId).Returns(existingReadModel);

        // Act
        await handler.HandleTalkScheduled(storedEvent);

        // Assert - should not update
        await repository.DidNotReceive().Update(Arg.Any<ConferenceTalkReadModel>());
    }

    [Fact]
    public async Task HandleTalkAssignedToRoom_OutOfOrderEvent_DoesNotUpdate()
    {
        // Arrange
        var repository = Substitute.For<IConferenceTalkReadModelRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.NewGuid();
        var roomId = Guid.NewGuid();

        var existingReadModel = new ConferenceTalkReadModel
        {
            Id = talkId.ToString(),
            RoomId = roomId.ToString(),
            RoomName = "Main Hall",
            Version = 3, // Already at version 3
        };

        var payload = new
        {
            AggregateId = Guid.NewGuid(),
            OccurredAt = DateTimeOffset.UtcNow,
            Version = 2L, // Older version - out of order
            Name = "Conference",
            Start = DateTimeOffset.UtcNow.AddDays(30),
            End = DateTimeOffset.UtcNow.AddDays(32),
            LocationName = "Location",
            Street = "Street",
            City = "City",
            State = "State",
            PostalCode = "12345",
            Country = "Country",
            OrganizerId = Guid.NewGuid(),
            Status = "Published",
            TalkId = talkId,
            RoomId = Guid.NewGuid(),
            RoomName = "Different Room",
        };

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TalkAssignedToRoomEvent",
            JsonSerializer.Serialize(payload),
            DateTimeOffset.UtcNow,
            2
        );

        repository.GetById(talkId).Returns(existingReadModel);

        // Act
        await handler.HandleTalkAssignedToRoom(storedEvent);

        // Assert - should not update
        await repository.DidNotReceive().Update(Arg.Any<ConferenceTalkReadModel>());
        Assert.Equal("Main Hall", existingReadModel.RoomName); // Room unchanged
        Assert.Equal(3, existingReadModel.Version); // Version unchanged
    }
}
