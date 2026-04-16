using System.Text.Json;
using ConferenceExample.EventStore;
using ConferenceExample.Talk.Persistence.EventHandlers;
using ConferenceExample.Talk.Persistence.ReadModels;
using NSubstitute;

namespace ConferenceExample.Talk.Persistence.UnitTests;

public class TalkEventHandlerTests
{
    [Fact]
    public async Task HandleTalkDomainEvent_NewTalk_CreatesReadModel()
    {
        // Arrange
        var repository = Substitute.For<ITalkReadModelRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.NewGuid();
        var speakerId = Guid.NewGuid();
        var talkTypeId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
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

        repository.GetById(talkId).Returns((TalkReadModel?)null);

        // Act
        await handler.HandleTalkDomainEvent(storedEvent);

        // Assert
        await repository
            .Received(1)
            .Save(
                Arg.Is<TalkReadModel>(rm =>
                    rm.Id == talkId.ToString()
                    && rm.Title == "Test Talk"
                    && rm.Abstract == "Test Abstract"
                    && rm.SpeakerId == speakerId.ToString()
                    && rm.Status == "Submitted"
                    && rm.Version == 1
                    && rm.Tags.Count == 2
                )
            );
    }

    [Fact]
    public async Task HandleTalkDomainEvent_ExistingTalk_UpdatesReadModel()
    {
        // Arrange
        var repository = Substitute.For<ITalkReadModelRepository>();
        var handler = new TalkEventHandler(repository);

        var talkId = Guid.NewGuid();
        var speakerId = Guid.NewGuid();
        var talkTypeId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        var existingReadModel = new TalkReadModel
        {
            Id = talkId.ToString(),
            Title = "Old Title",
            Abstract = "Old Abstract",
            Version = 1,
            Tags = new List<string>(),
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
                Arg.Is<TalkReadModel>(rm =>
                    rm.Id == talkId.ToString()
                    && rm.Title == "Updated Talk"
                    && rm.Abstract == "Updated Abstract"
                    && rm.Version == 2
                )
            );
    }

    [Fact]
    public async Task HandleTalkDomainEvent_InvalidPayload_ThrowsException()
    {
        // Arrange
        var repository = Substitute.For<ITalkReadModelRepository>();
        var handler = new TalkEventHandler(repository);

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TalkSubmittedEvent",
            "invalid json",
            DateTimeOffset.UtcNow,
            1
        );

        // Act & Assert
        await Assert.ThrowsAsync<System.Text.Json.JsonException>(async () =>
            await handler.HandleTalkDomainEvent(storedEvent)
        );
    }
}
