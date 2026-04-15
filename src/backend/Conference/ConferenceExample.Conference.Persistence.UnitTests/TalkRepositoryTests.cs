using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.EventStore;
using NSubstitute;

namespace ConferenceExample.Conference.Persistence.UnitTests;

public class TalkRepositoryTests
{
    [Fact]
    public async Task GetTalksByConferenceId_WithMatchingConference_ReturnsTalks()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        var talkId = GuidV7.NewGuid().Value;
        var speakerId = GuidV7.NewGuid().Value;
        var talkTypeId = GuidV7.NewGuid().Value;

        var talkSubmittedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId,
            "TalkSubmittedEvent",
            $$"""{"Title":"Test Talk","Abstract":"Test Abstract","SpeakerId":"{{speakerId}}","Tags":["tag1","tag2"],"TalkTypeId":"{{talkTypeId}}","ConferenceId":"{{conferenceId.Value.Value}}"}""",
            DateTimeOffset.UtcNow,
            0
        );

        eventStore.GetAllEvents().Returns(new List<StoredEvent> { talkSubmittedEvent });

        var repository = new TalkRepository(eventStore);

        // Act
        var result = await repository.GetTalksByConferenceId(conferenceId);

        // Assert
        Assert.Single(result);
        Assert.Equal("Test Talk", result[0].Title?.Value);
        Assert.Equal("Test Abstract", result[0].Abstract?.Value);
        Assert.Equal(speakerId, result[0].SpeakerId?.Value);
        Assert.Equal(talkTypeId, result[0].TalkTypeId?.Value);
        Assert.Equal(2, result[0].Tags.Count);
        Assert.Contains("tag1", result[0].Tags);
        Assert.Contains("tag2", result[0].Tags);
    }

    [Fact]
    public async Task GetTalksByConferenceId_WithDifferentConference_ReturnsEmpty()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        var differentConferenceId = GuidV7.NewGuid();
        var talkId = GuidV7.NewGuid().Value;

        var talkSubmittedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId,
            "TalkSubmittedEvent",
            $$"""{"Title":"Test Talk","Abstract":"Test Abstract","SpeakerId":"{{GuidV7.NewGuid().Value}}","Tags":[],"TalkTypeId":"{{GuidV7.NewGuid().Value}}","ConferenceId":"{{differentConferenceId.Value}}"}""",
            DateTimeOffset.UtcNow,
            0
        );

        eventStore.GetAllEvents().Returns(new List<StoredEvent> { talkSubmittedEvent });

        var repository = new TalkRepository(eventStore);

        // Act
        var result = await repository.GetTalksByConferenceId(conferenceId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTalksByConferenceId_WithTitleEdit_ReturnsUpdatedTitle()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        var talkId = GuidV7.NewGuid().Value;

        var talkSubmittedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId,
            "TalkSubmittedEvent",
            $$"""{"Title":"Original Title","Abstract":"Test Abstract","SpeakerId":"{{GuidV7.NewGuid().Value}}","Tags":[],"TalkTypeId":"{{GuidV7.NewGuid().Value}}","ConferenceId":"{{conferenceId.Value.Value}}"}""",
            DateTimeOffset.UtcNow,
            0
        );

        var titleEditedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId,
            "TalkTitleEditedEvent",
            """{"Title":"Updated Title"}""",
            DateTimeOffset.UtcNow,
            1
        );

        eventStore
            .GetAllEvents()
            .Returns(new List<StoredEvent> { talkSubmittedEvent, titleEditedEvent });

        var repository = new TalkRepository(eventStore);

        // Act
        var result = await repository.GetTalksByConferenceId(conferenceId);

        // Assert
        Assert.Single(result);
        Assert.Equal("Updated Title", result[0].Title?.Value);
    }

    [Fact]
    public async Task GetTalksByConferenceId_WithAbstractEdit_ReturnsUpdatedAbstract()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        var talkId = GuidV7.NewGuid().Value;

        var talkSubmittedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId,
            "TalkSubmittedEvent",
            $$"""{"Title":"Test Title","Abstract":"Original Abstract","SpeakerId":"{{GuidV7.NewGuid().Value}}","Tags":[],"TalkTypeId":"{{GuidV7.NewGuid().Value}}","ConferenceId":"{{conferenceId.Value.Value}}"}""",
            DateTimeOffset.UtcNow,
            0
        );

        var abstractEditedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId,
            "TalkAbstractEditedEvent",
            """{"Abstract":"Updated Abstract"}""",
            DateTimeOffset.UtcNow,
            1
        );

        eventStore
            .GetAllEvents()
            .Returns(new List<StoredEvent> { talkSubmittedEvent, abstractEditedEvent });

        var repository = new TalkRepository(eventStore);

        // Act
        var result = await repository.GetTalksByConferenceId(conferenceId);

        // Assert
        Assert.Single(result);
        Assert.Equal("Updated Abstract", result[0].Abstract?.Value);
    }

    [Fact]
    public async Task GetTalksByConferenceId_WithTagAdded_ReturnsWithAddedTag()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        var talkId = GuidV7.NewGuid().Value;

        var talkSubmittedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId,
            "TalkSubmittedEvent",
            $$"""{"Title":"Test Title","Abstract":"Test Abstract","SpeakerId":"{{GuidV7.NewGuid().Value}}","Tags":["tag1"],"TalkTypeId":"{{GuidV7.NewGuid().Value}}","ConferenceId":"{{conferenceId.Value.Value}}"}""",
            DateTimeOffset.UtcNow,
            0
        );

        var tagAddedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId,
            "TalkTagAddedEvent",
            """{"Tag":"tag2"}""",
            DateTimeOffset.UtcNow,
            1
        );

        eventStore
            .GetAllEvents()
            .Returns(new List<StoredEvent> { talkSubmittedEvent, tagAddedEvent });

        var repository = new TalkRepository(eventStore);

        // Act
        var result = await repository.GetTalksByConferenceId(conferenceId);

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result[0].Tags.Count);
        Assert.Contains("tag1", result[0].Tags);
        Assert.Contains("tag2", result[0].Tags);
    }

    [Fact]
    public async Task GetTalksByConferenceId_WithDuplicateTagAdded_DoesNotAddDuplicate()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        var talkId = GuidV7.NewGuid().Value;

        var talkSubmittedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId,
            "TalkSubmittedEvent",
            $$"""{"Title":"Test Title","Abstract":"Test Abstract","SpeakerId":"{{GuidV7.NewGuid().Value}}","Tags":["tag1"],"TalkTypeId":"{{GuidV7.NewGuid().Value}}","ConferenceId":"{{conferenceId.Value.Value}}"}""",
            DateTimeOffset.UtcNow,
            0
        );

        var tagAddedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId,
            "TalkTagAddedEvent",
            """{"Tag":"tag1"}""",
            DateTimeOffset.UtcNow,
            1
        );

        eventStore
            .GetAllEvents()
            .Returns(new List<StoredEvent> { talkSubmittedEvent, tagAddedEvent });

        var repository = new TalkRepository(eventStore);

        // Act
        var result = await repository.GetTalksByConferenceId(conferenceId);

        // Assert
        Assert.Single(result);
        Assert.Single(result[0].Tags);
        Assert.Contains("tag1", result[0].Tags);
    }

    [Fact]
    public async Task GetTalksByConferenceId_WithTagRemoved_ReturnsWithoutTag()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        var talkId = GuidV7.NewGuid().Value;

        var talkSubmittedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId,
            "TalkSubmittedEvent",
            $$"""{"Title":"Test Title","Abstract":"Test Abstract","SpeakerId":"{{GuidV7.NewGuid().Value}}","Tags":["tag1","tag2"],"TalkTypeId":"{{GuidV7.NewGuid().Value}}","ConferenceId":"{{conferenceId.Value.Value}}"}""",
            DateTimeOffset.UtcNow,
            0
        );

        var tagRemovedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId,
            "TalkTagRemovedEvent",
            """{"Tag":"tag1"}""",
            DateTimeOffset.UtcNow,
            1
        );

        eventStore
            .GetAllEvents()
            .Returns(new List<StoredEvent> { talkSubmittedEvent, tagRemovedEvent });

        var repository = new TalkRepository(eventStore);

        // Act
        var result = await repository.GetTalksByConferenceId(conferenceId);

        // Assert
        Assert.Single(result);
        Assert.Single(result[0].Tags);
        Assert.Contains("tag2", result[0].Tags);
        Assert.DoesNotContain("tag1", result[0].Tags);
    }

    [Fact]
    public async Task GetTalksByConferenceId_WithNoSubmittedEvent_SkipsTalk()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        var talkId = GuidV7.NewGuid().Value;

        var titleEditedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId,
            "TalkTitleEditedEvent",
            """{"Title":"Updated Title"}""",
            DateTimeOffset.UtcNow,
            0
        );

        eventStore.GetAllEvents().Returns(new List<StoredEvent> { titleEditedEvent });

        var repository = new TalkRepository(eventStore);

        // Act
        var result = await repository.GetTalksByConferenceId(conferenceId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTalksByConferenceId_WithNullSubmittedData_SkipsTalk()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        var talkId = GuidV7.NewGuid().Value;

        var invalidEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId,
            "TalkSubmittedEvent",
            "null",
            DateTimeOffset.UtcNow,
            0
        );

        eventStore.GetAllEvents().Returns(new List<StoredEvent> { invalidEvent });

        var repository = new TalkRepository(eventStore);

        // Act
        var result = await repository.GetTalksByConferenceId(conferenceId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTalksByConferenceId_WithNullTitleEditData_KeepsOriginalTitle()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        var talkId = GuidV7.NewGuid().Value;

        var talkSubmittedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId,
            "TalkSubmittedEvent",
            $$"""{"Title":"Original Title","Abstract":"Test Abstract","SpeakerId":"{{GuidV7.NewGuid().Value}}","Tags":[],"TalkTypeId":"{{GuidV7.NewGuid().Value}}","ConferenceId":"{{conferenceId.Value.Value}}"}""",
            DateTimeOffset.UtcNow,
            0
        );

        var invalidTitleEdit = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId,
            "TalkTitleEditedEvent",
            "null",
            DateTimeOffset.UtcNow,
            1
        );

        eventStore
            .GetAllEvents()
            .Returns(new List<StoredEvent> { talkSubmittedEvent, invalidTitleEdit });

        var repository = new TalkRepository(eventStore);

        // Act
        var result = await repository.GetTalksByConferenceId(conferenceId);

        // Assert
        Assert.Single(result);
        Assert.Equal("Original Title", result[0].Title?.Value);
    }

    [Fact]
    public async Task GetTalksByConferenceId_WithNullAbstractEditData_KeepsOriginalAbstract()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        var talkId = GuidV7.NewGuid().Value;

        var talkSubmittedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId,
            "TalkSubmittedEvent",
            $$"""{"Title":"Test Title","Abstract":"Original Abstract","SpeakerId":"{{GuidV7.NewGuid().Value}}","Tags":[],"TalkTypeId":"{{GuidV7.NewGuid().Value}}","ConferenceId":"{{conferenceId.Value.Value}}"}""",
            DateTimeOffset.UtcNow,
            0
        );

        var invalidAbstractEdit = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId,
            "TalkAbstractEditedEvent",
            "null",
            DateTimeOffset.UtcNow,
            1
        );

        eventStore
            .GetAllEvents()
            .Returns(new List<StoredEvent> { talkSubmittedEvent, invalidAbstractEdit });

        var repository = new TalkRepository(eventStore);

        // Act
        var result = await repository.GetTalksByConferenceId(conferenceId);

        // Assert
        Assert.Single(result);
        Assert.Equal("Original Abstract", result[0].Abstract?.Value);
    }

    [Fact]
    public async Task GetTalksByConferenceId_WithNullTagAddedData_DoesNotAddTag()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        var talkId = GuidV7.NewGuid().Value;

        var talkSubmittedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId,
            "TalkSubmittedEvent",
            $$"""{"Title":"Test Title","Abstract":"Test Abstract","SpeakerId":"{{GuidV7.NewGuid().Value}}","Tags":["tag1"],"TalkTypeId":"{{GuidV7.NewGuid().Value}}","ConferenceId":"{{conferenceId.Value.Value}}"}""",
            DateTimeOffset.UtcNow,
            0
        );

        var invalidTagAdded = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId,
            "TalkTagAddedEvent",
            "null",
            DateTimeOffset.UtcNow,
            1
        );

        eventStore
            .GetAllEvents()
            .Returns(new List<StoredEvent> { talkSubmittedEvent, invalidTagAdded });

        var repository = new TalkRepository(eventStore);

        // Act
        var result = await repository.GetTalksByConferenceId(conferenceId);

        // Assert
        Assert.Single(result);
        Assert.Single(result[0].Tags);
        Assert.Contains("tag1", result[0].Tags);
    }

    [Fact]
    public async Task GetTalksByConferenceId_WithNullTagRemovedData_DoesNotRemoveTag()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        var talkId = GuidV7.NewGuid().Value;

        var talkSubmittedEvent = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId,
            "TalkSubmittedEvent",
            $$"""{"Title":"Test Title","Abstract":"Test Abstract","SpeakerId":"{{GuidV7.NewGuid().Value}}","Tags":["tag1","tag2"],"TalkTypeId":"{{GuidV7.NewGuid().Value}}","ConferenceId":"{{conferenceId.Value.Value}}"}""",
            DateTimeOffset.UtcNow,
            0
        );

        var invalidTagRemoved = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId,
            "TalkTagRemovedEvent",
            "null",
            DateTimeOffset.UtcNow,
            1
        );

        eventStore
            .GetAllEvents()
            .Returns(new List<StoredEvent> { talkSubmittedEvent, invalidTagRemoved });

        var repository = new TalkRepository(eventStore);

        // Act
        var result = await repository.GetTalksByConferenceId(conferenceId);

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result[0].Tags.Count);
        Assert.Contains("tag1", result[0].Tags);
        Assert.Contains("tag2", result[0].Tags);
    }

    [Fact]
    public async Task GetTalksByConferenceId_WithNoEvents_ReturnsEmpty()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());

        eventStore.GetAllEvents().Returns(new List<StoredEvent>());

        var repository = new TalkRepository(eventStore);

        // Act
        var result = await repository.GetTalksByConferenceId(conferenceId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTalksByConferenceId_WithMultipleTalks_ReturnsAllMatchingTalks()
    {
        // Arrange
        var eventStore = Substitute.For<IEventStore>();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        var talkId1 = GuidV7.NewGuid().Value;
        var talkId2 = GuidV7.NewGuid().Value;

        var talkSubmittedEvent1 = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId1,
            "TalkSubmittedEvent",
            $$"""{"Title":"Talk 1","Abstract":"Abstract 1","SpeakerId":"{{GuidV7.NewGuid().Value}}","Tags":[],"TalkTypeId":"{{GuidV7.NewGuid().Value}}","ConferenceId":"{{conferenceId.Value.Value}}"}""",
            DateTimeOffset.UtcNow,
            0
        );

        var talkSubmittedEvent2 = new StoredEvent(
            GuidV7.NewGuid().Value,
            talkId2,
            "TalkSubmittedEvent",
            $$"""{"Title":"Talk 2","Abstract":"Abstract 2","SpeakerId":"{{GuidV7.NewGuid().Value}}","Tags":[],"TalkTypeId":"{{GuidV7.NewGuid().Value}}","ConferenceId":"{{conferenceId.Value.Value}}"}""",
            DateTimeOffset.UtcNow,
            0
        );

        eventStore
            .GetAllEvents()
            .Returns(new List<StoredEvent> { talkSubmittedEvent1, talkSubmittedEvent2 });

        var repository = new TalkRepository(eventStore);

        // Act
        var result = await repository.GetTalksByConferenceId(conferenceId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Title?.Value == "Talk 1");
        Assert.Contains(result, t => t.Title?.Value == "Talk 2");
    }
}
