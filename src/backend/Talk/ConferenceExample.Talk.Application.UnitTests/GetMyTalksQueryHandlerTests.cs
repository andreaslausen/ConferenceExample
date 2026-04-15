using ConferenceExample.Authentication;
using ConferenceExample.Talk.Application.GetMyTalks;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.TalkManagement;
using NSubstitute;
using AuthGuidV7 = ConferenceExample.Authentication.SharedKernel.ValueObjects.Ids.GuidV7;
using TalkEntity = ConferenceExample.Talk.Domain.TalkManagement.Talk;

namespace ConferenceExample.Talk.Application.UnitTests;

public class GetMyTalksQueryHandlerTests
{
    private static TalkEntity CreateTalk(SpeakerId speakerId, string title)
    {
        return TalkEntity.Submit(
            new TalkId(GuidV7.NewGuid()),
            new TalkTitle(title),
            speakerId,
            new List<TalkTag> { new("tag1"), new("tag2") },
            new TalkTypeId(GuidV7.NewGuid()),
            new Abstract("Test Abstract"),
            new ConferenceId(GuidV7.NewGuid())
        );
    }

    [Fact]
    public async Task Handle_ReturnsTalksForCurrentSpeaker()
    {
        // Arrange
        var talkRepository = Substitute.For<ITalkRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var userId = new UserId(AuthGuidV7.NewGuid());
        currentUserService.GetCurrentUserId().Returns(userId);

        var speakerId = new SpeakerId(new GuidV7(userId.Value.Value));
        var talk1 = CreateTalk(speakerId, "Talk 1");
        var talk2 = CreateTalk(speakerId, "Talk 2");
        talkRepository.GetTalksBySpeaker(speakerId).Returns(new List<TalkEntity> { talk1, talk2 });

        var handler = new GetMyTalksQueryHandler(talkRepository, currentUserService);
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
        var talkRepository = Substitute.For<ITalkRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var userId = new UserId(AuthGuidV7.NewGuid());
        currentUserService.GetCurrentUserId().Returns(userId);

        var speakerId = new SpeakerId(new GuidV7(userId.Value.Value));
        talkRepository.GetTalksBySpeaker(speakerId).Returns(new List<TalkEntity>());

        var handler = new GetMyTalksQueryHandler(talkRepository, currentUserService);
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
        var talkRepository = Substitute.For<ITalkRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var userId = new UserId(AuthGuidV7.NewGuid());
        currentUserService.GetCurrentUserId().Returns(userId);

        var speakerId = new SpeakerId(new GuidV7(userId.Value.Value));
        var talk = CreateTalk(speakerId, "Test Talk");
        talkRepository.GetTalksBySpeaker(speakerId).Returns(new List<TalkEntity> { talk });

        var handler = new GetMyTalksQueryHandler(talkRepository, currentUserService);
        var query = new GetMyTalksQuery();

        // Act
        var result = await handler.Handle(query);

        // Assert
        var dto = Assert.Single(result);
        Assert.Equal(talk.Id.Value.Value, dto.Id);
        Assert.Equal(talk.Title.Title, dto.Title);
        Assert.Equal(talk.Abstract.Content, dto.Abstract);
        Assert.Equal(talk.ConferenceId.Value.Value, dto.ConferenceId);
        Assert.Equal(talk.Status.ToString(), dto.Status);
        Assert.Equal(2, dto.Tags.Count);
        Assert.Contains("tag1", dto.Tags);
        Assert.Contains("tag2", dto.Tags);
    }

    [Fact]
    public async Task Handle_UsesCorrectCurrentUserId()
    {
        // Arrange
        var talkRepository = Substitute.For<ITalkRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var userId = new UserId(AuthGuidV7.NewGuid());
        currentUserService.GetCurrentUserId().Returns(userId);

        var expectedSpeakerId = new SpeakerId(new GuidV7(userId.Value.Value));
        talkRepository.GetTalksBySpeaker(Arg.Any<SpeakerId>()).Returns(new List<TalkEntity>());

        var handler = new GetMyTalksQueryHandler(talkRepository, currentUserService);
        var query = new GetMyTalksQuery();

        // Act
        await handler.Handle(query);

        // Assert
        await talkRepository.Received(1).GetTalksBySpeaker(expectedSpeakerId);
    }
}
