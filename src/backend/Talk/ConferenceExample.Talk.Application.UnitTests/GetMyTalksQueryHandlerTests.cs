using ConferenceExample.Talk.Application.GetMyTalks;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.TalkManagement;
using NSubstitute;

namespace ConferenceExample.Talk.Application.UnitTests;

public class GetMyTalksQueryHandlerTests
{
    private static TalkReadModel CreateSummary(Guid speakerId, string title) =>
        new(
            Guid.NewGuid(),
            title,
            "Test Abstract",
            Guid.NewGuid(),
            "Submitted",
            new List<string> { "tag1", "tag2" },
            speakerId,
            "Jane Doe"
        );

    [Fact]
    public async Task Handle_ReturnsTalksForCurrentSpeaker()
    {
        // Arrange
        var talkSummaryRepository = Substitute.For<ITalkReadModelRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var userId = Guid.CreateVersion7();
        currentUserService.GetCurrentUserId().Returns(userId);

        var speakerId = new SpeakerId(new GuidV7(userId));
        var summary1 = CreateSummary(speakerId.Value.Value, "Talk 1") with { Title = "Talk 1" };
        var summary2 = CreateSummary(speakerId.Value.Value, "Talk 2") with { Title = "Talk 2" };
        talkSummaryRepository
            .GetBySpeakerId(speakerId)
            .Returns(new List<TalkReadModel> { summary1, summary2 });

        var handler = new GetMyTalksQueryHandler(talkSummaryRepository, currentUserService);
        var query = new GetMyTalksQuery();

        // Act
        var result = await handler.Handle(query);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, dto => dto.Title == "Talk 1");
        Assert.Contains(result, dto => dto.Title == "Talk 2");
    }

    [Fact]
    public async Task Handle_ReturnsEmptyListWhenNoTalks()
    {
        // Arrange
        var talkSummaryRepository = Substitute.For<ITalkReadModelRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var userId = Guid.CreateVersion7();
        currentUserService.GetCurrentUserId().Returns(userId);

        var speakerId = new SpeakerId(new GuidV7(userId));
        talkSummaryRepository.GetBySpeakerId(speakerId).Returns(new List<TalkReadModel>());

        var handler = new GetMyTalksQueryHandler(talkSummaryRepository, currentUserService);
        var query = new GetMyTalksQuery();

        // Act
        var result = await handler.Handle(query);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_MapsPropertiesCorrectly()
    {
        // Arrange
        var talkSummaryRepository = Substitute.For<ITalkReadModelRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var userId = Guid.CreateVersion7();
        currentUserService.GetCurrentUserId().Returns(userId);

        var speakerId = new SpeakerId(new GuidV7(userId));
        var talkId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var summary = new TalkReadModel(
            talkId,
            "Test Talk",
            "Test Abstract",
            conferenceId,
            "Submitted",
            new List<string> { "tag1", "tag2" },
            speakerId.Value.Value,
            "Jane Doe"
        );
        talkSummaryRepository
            .GetBySpeakerId(speakerId)
            .Returns(new List<TalkReadModel> { summary });

        var handler = new GetMyTalksQueryHandler(talkSummaryRepository, currentUserService);
        var query = new GetMyTalksQuery();

        // Act
        var result = await handler.Handle(query);

        // Assert
        var dto = Assert.Single(result);
        Assert.Equal(talkId, dto.Id);
        Assert.Equal("Test Talk", dto.Title);
        Assert.Equal("Test Abstract", dto.Abstract);
        Assert.Equal(conferenceId, dto.ConferenceId);
        Assert.Equal("Submitted", dto.Status);
        Assert.Equal(2, dto.Tags.Count);
        Assert.Contains("tag1", dto.Tags);
        Assert.Contains("tag2", dto.Tags);
    }

    [Fact]
    public async Task Handle_UsesCorrectCurrentUserId()
    {
        // Arrange
        var talkSummaryRepository = Substitute.For<ITalkReadModelRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var userId = Guid.CreateVersion7();
        currentUserService.GetCurrentUserId().Returns(userId);

        var expectedSpeakerId = new SpeakerId(new GuidV7(userId));
        talkSummaryRepository
            .GetBySpeakerId(Arg.Any<SpeakerId>())
            .Returns(new List<TalkReadModel>());

        var handler = new GetMyTalksQueryHandler(talkSummaryRepository, currentUserService);
        var query = new GetMyTalksQuery();

        // Act
        await handler.Handle(query);

        // Assert
        await talkSummaryRepository.Received(1).GetBySpeakerId(expectedSpeakerId);
    }
}
