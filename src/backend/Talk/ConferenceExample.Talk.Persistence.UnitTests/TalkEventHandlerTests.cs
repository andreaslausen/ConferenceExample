using System.Text.Json;
using ConferenceExample.EventStore;
using ConferenceExample.Talk.Persistence.EventHandlers;
using ConferenceExample.Talk.Persistence.ReadModels;
using NSubstitute;

namespace ConferenceExample.Talk.Persistence.UnitTests;

public class TalkEventHandlerTests
{
    [Fact]
    public async Task HandleTalkSubmitted_CreatesReadModel()
    {
        var repository = Substitute.For<ITalkDocumentRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.CreateVersion7();
        var speakerId = Guid.CreateVersion7();
        var talkTypeId = Guid.CreateVersion7();
        var conferenceId = Guid.CreateVersion7();
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
                Arg.Is<TalkDocument>(rm =>
                    rm.Id == talkId.ToString()
                    && rm.Title == "Test Talk"
                    && rm.Abstract == "Test Abstract"
                    && rm.SpeakerId == speakerId.ToString()
                    && rm.Status == "Submitted"
                    && rm.Version == 0
                    && rm.Tags.Count == 2
                )
            );
    }

    [Fact]
    public async Task HandleTalkTitleEdited_UpdatesTitle()
    {
        var repository = Substitute.For<ITalkDocumentRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.CreateVersion7();
        var existing = new TalkDocument
        {
            Id = talkId.ToString(),
            Title = "Old Title",
            Abstract = "Abstract",
            Version = 0,
            Tags = [],
        };
        repository.GetById(talkId).Returns(existing);

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            talkId,
            "TalkTitleEditedEvent",
            JsonSerializer.Serialize(new { Title = "New Title" }),
            DateTimeOffset.UtcNow,
            1
        );

        await handler.HandleTalkTitleEdited(storedEvent);

        await repository
            .Received(1)
            .Update(
                Arg.Is<TalkDocument>(rm =>
                    rm.Title == "New Title" && rm.Abstract == "Abstract" && rm.Version == 1
                )
            );
    }

    [Fact]
    public async Task HandleTalkAbstractEdited_UpdatesAbstract()
    {
        var repository = Substitute.For<ITalkDocumentRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.CreateVersion7();
        var existing = new TalkDocument
        {
            Id = talkId.ToString(),
            Title = "Title",
            Abstract = "Old Abstract",
            Version = 0,
            Tags = [],
        };
        repository.GetById(talkId).Returns(existing);

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            talkId,
            "TalkAbstractEditedEvent",
            JsonSerializer.Serialize(new { Abstract = "New Abstract" }),
            DateTimeOffset.UtcNow,
            1
        );

        await handler.HandleTalkAbstractEdited(storedEvent);

        await repository
            .Received(1)
            .Update(Arg.Is<TalkDocument>(rm => rm.Abstract == "New Abstract" && rm.Version == 1));
    }

    [Fact]
    public async Task HandleTalkTagAdded_AppendsTag()
    {
        var repository = Substitute.For<ITalkDocumentRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.CreateVersion7();
        var existing = new TalkDocument
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
                Arg.Is<TalkDocument>(rm =>
                    rm.Tags.Count == 2 && rm.Tags.Contains("csharp") && rm.Version == 1
                )
            );
    }

    [Fact]
    public async Task HandleTalkTagRemoved_DropsTag()
    {
        var repository = Substitute.For<ITalkDocumentRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.CreateVersion7();
        var existing = new TalkDocument
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
                Arg.Is<TalkDocument>(rm =>
                    rm.Tags.Count == 1 && !rm.Tags.Contains("dotnet") && rm.Version == 1
                )
            );
    }

    [Fact]
    public async Task HandleTalkTitleEdited_MissingReadModel_DoesNothing()
    {
        var repository = Substitute.For<ITalkDocumentRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.CreateVersion7();
        repository.GetById(talkId).Returns((TalkDocument?)null);

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            talkId,
            "TalkTitleEditedEvent",
            JsonSerializer.Serialize(new { Title = "New Title" }),
            DateTimeOffset.UtcNow,
            1
        );

        await handler.HandleTalkTitleEdited(storedEvent);

        await repository.DidNotReceive().Update(Arg.Any<TalkDocument>());
    }
}
