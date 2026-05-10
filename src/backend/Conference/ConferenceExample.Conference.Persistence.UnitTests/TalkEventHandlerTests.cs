using System.Text.Json;
using ConferenceExample.Conference.Persistence.EventHandlers;
using ConferenceExample.Conference.Persistence.ReadModels;
using ConferenceExample.EventStore;
using NSubstitute;

namespace ConferenceExample.Conference.Persistence.UnitTests;

public class TalkEventHandlerTests
{
    [Fact]
    public async Task HandleTalkSubmitted_CreatesReadModel()
    {
        var repository = Substitute.For<IConferenceTalkDocumentRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.CreateVersion7();
        var conferenceId = Guid.CreateVersion7();
        var speakerId = Guid.CreateVersion7();
        var talkTypeId = Guid.CreateVersion7();
        var occurredAt = DateTimeOffset.UtcNow;

        var payload = new
        {
            Title = "Test Talk",
            Abstract = "Test Abstract",
            SpeakerId = speakerId,
            SpeakerFirstName = "Jane",
            SpeakerLastName = "Doe",
            SpeakerBiography = "Speaker bio",
            Tags = new List<string> { "dotnet", "testing" },
            TalkTypeId = talkTypeId,
            ConferenceId = conferenceId,
            Status = "Submitted",
        };

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            talkId,
            "TalkSubmittedEvent",
            JsonSerializer.Serialize(payload),
            occurredAt,
            0
        );

        await handler.HandleTalkSubmitted(storedEvent);

        await repository
            .Received(1)
            .Save(
                Arg.Is<ConferenceTalkDocument>(rm =>
                    rm.Id == talkId.ToString()
                    && rm.Title == "Test Talk"
                    && rm.ConferenceId == conferenceId.ToString()
                    && rm.Status == "Submitted"
                    && rm.Version == 0
                )
            );
    }

    [Fact]
    public async Task HandleTalkTitleEdited_UpdatesTitle()
    {
        var repository = Substitute.For<IConferenceTalkDocumentRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.CreateVersion7();
        var existing = new ConferenceTalkDocument
        {
            Id = talkId.ToString(),
            Title = "Old",
            Status = "Submitted",
            Version = 0,
        };
        repository.GetById(talkId).Returns(existing);

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            talkId,
            "TalkTitleEditedEvent",
            JsonSerializer.Serialize(new { Title = "Brand New" }),
            DateTimeOffset.UtcNow,
            1
        );

        await handler.HandleTalkTitleEdited(storedEvent);

        await repository
            .Received(1)
            .Update(
                Arg.Is<ConferenceTalkDocument>(rm =>
                    rm.Title == "Brand New" && rm.Version == 1
                )
            );
    }

    [Fact]
    public async Task HandleTalkAccepted_SetsStatusToAccepted()
    {
        var repository = Substitute.For<IConferenceTalkDocumentRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.CreateVersion7();
        var existing = new ConferenceTalkDocument
        {
            Id = talkId.ToString(),
            Status = "Submitted",
            Version = 0,
        };
        repository.GetById(talkId).Returns(existing);

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "TalkAcceptedEvent",
            JsonSerializer.Serialize(new { TalkId = talkId }),
            DateTimeOffset.UtcNow,
            1
        );

        await handler.HandleTalkAccepted(storedEvent);

        await repository
            .Received(1)
            .Update(
                Arg.Is<ConferenceTalkDocument>(rm =>
                    rm.Id == talkId.ToString() && rm.Status == "Accepted"
                )
            );
    }

    [Fact]
    public async Task HandleTalkRejected_SetsStatusToRejected()
    {
        var repository = Substitute.For<IConferenceTalkDocumentRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.CreateVersion7();
        var existing = new ConferenceTalkDocument
        {
            Id = talkId.ToString(),
            Status = "Submitted",
            Version = 0,
        };
        repository.GetById(talkId).Returns(existing);

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "TalkRejectedEvent",
            JsonSerializer.Serialize(new { TalkId = talkId }),
            DateTimeOffset.UtcNow,
            1
        );

        await handler.HandleTalkRejected(storedEvent);

        await repository
            .Received(1)
            .Update(
                Arg.Is<ConferenceTalkDocument>(rm =>
                    rm.Id == talkId.ToString() && rm.Status == "Rejected"
                )
            );
    }

    [Fact]
    public async Task HandleTalkScheduled_UpdatesSlotTimes()
    {
        var repository = Substitute.For<IConferenceTalkDocumentRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.CreateVersion7();
        var talkStart = DateTimeOffset.UtcNow.AddDays(30);
        var talkEnd = talkStart.AddHours(1);

        var existing = new ConferenceTalkDocument
        {
            Id = talkId.ToString(),
            Status = "Accepted",
            Version = 1,
        };
        repository.GetById(talkId).Returns(existing);

        var payload = new
        {
            TalkId = talkId,
            TalkStart = talkStart,
            TalkEnd = talkEnd,
        };

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "TalkScheduledEvent",
            JsonSerializer.Serialize(payload),
            DateTimeOffset.UtcNow,
            2
        );

        await handler.HandleTalkScheduled(storedEvent);

        await repository
            .Received(1)
            .Update(
                Arg.Is<ConferenceTalkDocument>(rm =>
                    rm.Id == talkId.ToString()
                    && rm.SlotStart == talkStart
                    && rm.SlotEnd == talkEnd
                )
            );
    }

    [Fact]
    public async Task HandleTalkAssignedToRoom_UpdatesRoomInfo()
    {
        var repository = Substitute.For<IConferenceTalkDocumentRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.CreateVersion7();
        var roomId = Guid.CreateVersion7();
        var existing = new ConferenceTalkDocument
        {
            Id = talkId.ToString(),
            Status = "Accepted",
            Version = 1,
        };
        repository.GetById(talkId).Returns(existing);

        var payload = new
        {
            TalkId = talkId,
            RoomId = roomId,
            RoomName = "Main Hall",
        };

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "TalkAssignedToRoomEvent",
            JsonSerializer.Serialize(payload),
            DateTimeOffset.UtcNow,
            2
        );

        await handler.HandleTalkAssignedToRoom(storedEvent);

        await repository
            .Received(1)
            .Update(
                Arg.Is<ConferenceTalkDocument>(rm =>
                    rm.Id == talkId.ToString()
                    && rm.RoomId == roomId.ToString()
                    && rm.RoomName == "Main Hall"
                )
            );
    }

    [Fact]
    public async Task HandleTalkAccepted_NonExistingReadModel_DoesNotUpdate()
    {
        var repository = Substitute.For<IConferenceTalkDocumentRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.CreateVersion7();
        repository.GetById(talkId).Returns((ConferenceTalkDocument?)null);

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "TalkAcceptedEvent",
            JsonSerializer.Serialize(new { TalkId = talkId }),
            DateTimeOffset.UtcNow,
            1
        );

        await handler.HandleTalkAccepted(storedEvent);

        await repository.DidNotReceive().Update(Arg.Any<ConferenceTalkDocument>());
    }

    [Fact]
    public async Task HandleTalkTagAdded_AppendsTag()
    {
        var repository = Substitute.For<IConferenceTalkDocumentRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.CreateVersion7();
        var existing = new ConferenceTalkDocument
        {
            Id = talkId.ToString(),
            Tags = ["dotnet"],
            Version = 0,
        };
        repository.GetById(talkId).Returns(existing);

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            talkId,
            "TalkTagAddedEvent",
            JsonSerializer.Serialize(new { Tag = "csharp" }),
            DateTimeOffset.UtcNow,
            1
        );

        await handler.HandleTalkTagAdded(storedEvent);

        await repository
            .Received(1)
            .Update(
                Arg.Is<ConferenceTalkDocument>(rm =>
                    rm.Tags.Count == 2 && rm.Tags.Contains("csharp") && rm.Version == 1
                )
            );
    }

    [Fact]
    public async Task HandleTalkTagRemoved_DropsTag()
    {
        var repository = Substitute.For<IConferenceTalkDocumentRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.CreateVersion7();
        var existing = new ConferenceTalkDocument
        {
            Id = talkId.ToString(),
            Tags = ["dotnet", "csharp"],
            Version = 0,
        };
        repository.GetById(talkId).Returns(existing);

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            talkId,
            "TalkTagRemovedEvent",
            JsonSerializer.Serialize(new { Tag = "dotnet" }),
            DateTimeOffset.UtcNow,
            1
        );

        await handler.HandleTalkTagRemoved(storedEvent);

        await repository
            .Received(1)
            .Update(
                Arg.Is<ConferenceTalkDocument>(rm =>
                    rm.Tags.Count == 1 && !rm.Tags.Contains("dotnet")
                )
            );
    }

    [Fact]
    public async Task HandleTalkAbstractEdited_UpdatesAbstract()
    {
        var repository = Substitute.For<IConferenceTalkDocumentRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.CreateVersion7();
        var existing = new ConferenceTalkDocument
        {
            Id = talkId.ToString(),
            Abstract = "Old",
            Version = 0,
        };
        repository.GetById(talkId).Returns(existing);

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            talkId,
            "TalkAbstractEditedEvent",
            JsonSerializer.Serialize(new { Abstract = "New" }),
            DateTimeOffset.UtcNow,
            1
        );

        await handler.HandleTalkAbstractEdited(storedEvent);

        await repository
            .Received(1)
            .Update(Arg.Is<ConferenceTalkDocument>(rm => rm.Abstract == "New" && rm.Version == 1));
    }
}
