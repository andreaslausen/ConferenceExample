using ConferenceExample.Authentication;
using ConferenceExample.Talk.Application.EditTalk;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.TalkManagement;
using NSubstitute;
using AuthGuidV7 = ConferenceExample.Authentication.SharedKernel.ValueObjects.Ids.GuidV7;
using TalkEntity = ConferenceExample.Talk.Domain.TalkManagement.Talk;

namespace ConferenceExample.Talk.Application.UnitTests;

public class EditTalkCommandHandlerTests
{
    private static TalkEntity CreateValidTalk(SpeakerId speakerId)
    {
        return TalkEntity.Submit(
            new TalkId(GuidV7.NewGuid()),
            new TalkTitle("Original Title"),
            speakerId,
            new List<TalkTag> { new("oldtag") },
            new TalkTypeId(GuidV7.NewGuid()),
            new Abstract("Original Abstract"),
            new ConferenceId(GuidV7.NewGuid())
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesTalkAndSaves()
    {
        // Arrange
        var talkRepository = Substitute.For<ITalkRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var userId = new UserId(AuthGuidV7.NewGuid());
        currentUserService.GetCurrentUserId().Returns(userId);

        var speakerId = new SpeakerId(new GuidV7(userId.Value.Value));
        var talk = CreateValidTalk(speakerId);
        talkRepository.GetById(talk.Id).Returns(talk);

        var handler = new EditTalkCommandHandler(talkRepository, currentUserService);
        var command = new EditTalkCommand(
            talk.Id.Value,
            "Updated Title",
            "Updated Abstract",
            new List<string> { "newtag1", "newtag2" }
        );

        // Act
        await handler.Handle(command);

        // Assert
        await talkRepository.Received(1).Save(talk);
        Assert.Equal("Updated Title", talk.Title.Title);
        Assert.Equal("Updated Abstract", talk.Abstract.Content);
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var talkRepository = Substitute.For<ITalkRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var currentUserId = new UserId(AuthGuidV7.NewGuid());
        currentUserService.GetCurrentUserId().Returns(currentUserId);

        // Create talk with different speaker
        var differentSpeakerId = new SpeakerId(GuidV7.NewGuid());
        var talk = CreateValidTalk(differentSpeakerId);
        talkRepository.GetById(talk.Id).Returns(talk);

        var handler = new EditTalkCommandHandler(talkRepository, currentUserService);
        var command = new EditTalkCommand(
            talk.Id.Value,
            "Updated Title",
            "Updated Abstract",
            new List<string> { "tag1" }
        );

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await handler.Handle(command)
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ReplacesAllTags()
    {
        // Arrange
        var talkRepository = Substitute.For<ITalkRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var userId = new UserId(AuthGuidV7.NewGuid());
        currentUserService.GetCurrentUserId().Returns(userId);

        var speakerId = new SpeakerId(new GuidV7(userId.Value.Value));
        var talk = CreateValidTalk(speakerId);
        talkRepository.GetById(talk.Id).Returns(talk);

        var handler = new EditTalkCommandHandler(talkRepository, currentUserService);
        var command = new EditTalkCommand(
            talk.Id.Value,
            "Updated Title",
            "Updated Abstract",
            new List<string> { "newtag1", "newtag2", "newtag3" }
        );

        // Act
        await handler.Handle(command);

        // Assert
        Assert.Equal(3, talk.Tags.Count);
        Assert.Contains(talk.Tags, t => t.Tag == "newtag1");
        Assert.Contains(talk.Tags, t => t.Tag == "newtag2");
        Assert.Contains(talk.Tags, t => t.Tag == "newtag3");
        Assert.DoesNotContain(talk.Tags, t => t.Tag == "oldtag");
    }

    [Fact]
    public async Task Handle_EmptyTagList_RemovesAllTags()
    {
        // Arrange
        var talkRepository = Substitute.For<ITalkRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var userId = new UserId(AuthGuidV7.NewGuid());
        currentUserService.GetCurrentUserId().Returns(userId);

        var speakerId = new SpeakerId(new GuidV7(userId.Value.Value));
        var talk = CreateValidTalk(speakerId);
        talkRepository.GetById(talk.Id).Returns(talk);

        var handler = new EditTalkCommandHandler(talkRepository, currentUserService);
        var command = new EditTalkCommand(
            talk.Id.Value,
            "Updated Title",
            "Updated Abstract",
            new List<string>()
        );

        // Act
        await handler.Handle(command);

        // Assert
        Assert.Empty(talk.Tags);
    }
}
