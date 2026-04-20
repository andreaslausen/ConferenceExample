using ConferenceExample.EventStore;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.TalkManagement;
using NSubstitute;
using TalkEntity = ConferenceExample.Talk.Domain.TalkManagement.Talk;

namespace ConferenceExample.Talk.Persistence.UnitTests;

public class TalkRepositoryTests
{
    private static TalkEntity CreateValidTalk(ConferenceId? conferenceId = null)
    {
        return TalkEntity.Submit(
            new TalkId(GuidV7.NewGuid()),
            new TalkTitle("Test Title"),
            new SpeakerId(GuidV7.NewGuid()),
            new List<TalkTag> { new("dotnet") },
            new TalkTypeId(GuidV7.NewGuid()),
            new Abstract("Test Abstract"),
            conferenceId ?? new ConferenceId(GuidV7.NewGuid())
        );
    }

    [Fact]
    public async Task Save_NewTalk_AppendsSerializedEventsToEventStore()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var repo = new TalkRepository(eventStore);
        var talk = CreateValidTalk();

        // Act
        await repo.Save(talk);

        // Assert
        await eventStore
            .Received(1)
            .AppendEvents(
                talk.Id.Value,
                Arg.Is<IEnumerable<StoredEvent>>(events =>
                    events.Count() == 1
                    && events.First().AggregateId == (Guid)talk.Id.Value
                    && events.First().EventType == "TalkSubmittedEvent"
                ),
                Arg.Any<long>()
            );
    }

    [Fact]
    public async Task Save_ClearsUncommittedEventsAfterSaving()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var repo = new TalkRepository(eventStore);
        var talk = CreateValidTalk();

        // Act
        await repo.Save(talk);

        // Assert
        Assert.Empty(talk.GetUncommittedEvents());
    }

    [Fact]
    public async Task Save_NoUncommittedEvents_DoesNothing()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var repo = new TalkRepository(eventStore);

        var talk = CreateValidTalk();
        talk.ClearUncommittedEvents();

        // Act
        await repo.Save(talk);

        // Assert
        await eventStore
            .DidNotReceive()
            .AppendEvents(Arg.Any<Guid>(), Arg.Any<IEnumerable<StoredEvent>>(), Arg.Any<long>());
    }

    [Fact]
    public async Task GetById_ExistingTalk_RebuildsTalkFromEvents()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var talkId = new TalkId(GuidV7.NewGuid());
        var speakerId = new SpeakerId(GuidV7.NewGuid());
        var talkTypeId = new TalkTypeId(GuidV7.NewGuid());
        var conferenceId = new ConferenceId(GuidV7.NewGuid());

        var submittedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId.Value,
            "TalkSubmittedEvent",
            $$"""{"AggregateId":"{{talkId.Value}}","OccurredAt":"{{DateTimeOffset.UtcNow:O}}","Version":0,"Title":"Test Title","Abstract":"Test Abstract","SpeakerId":"{{speakerId.Value}}","Tags":["dotnet"],"TalkTypeId":"{{talkTypeId.Value}}","ConferenceId":"{{conferenceId.Value}}","Status":"Submitted"}""",
            DateTimeOffset.UtcNow,
            0
        );
        var titleEditedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId.Value,
            "TalkTitleEditedEvent",
            $$"""{"AggregateId":"{{talkId.Value}}","OccurredAt":"{{DateTimeOffset.UtcNow:O}}","Version":1,"Title":"Updated Title","Abstract":"Test Abstract","SpeakerId":"{{speakerId.Value}}","Tags":["dotnet"],"TalkTypeId":"{{talkTypeId.Value}}","ConferenceId":"{{conferenceId.Value}}","Status":"Submitted"}""",
            DateTimeOffset.UtcNow,
            1
        );

        eventStore
            .GetEvents(talkId.Value)
            .Returns(new List<StoredEvent> { submittedEvent, titleEditedEvent });

        var repo = new TalkRepository(eventStore);

        // Act
        var loaded = await repo.GetById(talkId);

        // Assert
        Assert.Equal(talkId, loaded.Id);
        Assert.Equal(new TalkTitle("Updated Title"), loaded.Title);
        Assert.Equal(conferenceId, loaded.ConferenceId);
    }

    [Fact]
    public async Task GetById_UnknownTalk_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var talkId = new TalkId(GuidV7.NewGuid());

        eventStore.GetEvents(talkId.Value).Returns(new List<StoredEvent>());

        var repo = new TalkRepository(eventStore);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetById(talkId));
    }

    [Fact]
    public async Task GetById_UnknownEventType_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var repo = new TalkRepository(eventStore);

        var talkId = new TalkId(GuidV7.NewGuid());
        var invalidEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId.Value,
            "UnknownEventType",
            "{}",
            DateTimeOffset.UtcNow,
            1
        );

        eventStore.GetEvents(talkId.Value).Returns([invalidEvent]);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.GetById(talkId)
        );
        Assert.Contains("Unknown event type: UnknownEventType", exception.Message);
    }

    [Fact]
    public async Task Save_WhenEventStoreThrowsConcurrencyException_PropagatesException()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var repo = new TalkRepository(eventStore);
        var talk = CreateValidTalk();

        eventStore
            .AppendEvents(Arg.Any<Guid>(), Arg.Any<IEnumerable<StoredEvent>>(), Arg.Any<long>())
            .Returns(Task.FromException(new ConcurrencyException("Version conflict")));

        // Act & Assert
        await Assert.ThrowsAsync<ConcurrencyException>(() => repo.Save(talk));
    }

    [Fact]
    public async Task GetById_InvalidPayload_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var repo = new TalkRepository(eventStore);

        var talkId = new TalkId(GuidV7.NewGuid());
        var eventWithInvalidPayload = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId.Value,
            "TalkSubmittedEvent",
            "null",
            DateTimeOffset.UtcNow,
            1
        );

        eventStore.GetEvents(talkId.Value).Returns([eventWithInvalidPayload]);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.GetById(talkId)
        );
        Assert.Contains("Failed to deserialize event: TalkSubmittedEvent", exception.Message);
    }
}
