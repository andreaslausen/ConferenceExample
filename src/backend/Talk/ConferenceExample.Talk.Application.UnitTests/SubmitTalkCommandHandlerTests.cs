using ConferenceExample.Authentication;
using ConferenceExample.Talk.Application.SubmitTalk;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.TalkManagement;
using NSubstitute;
using AuthGuidV7 = ConferenceExample.Authentication.SharedKernel.ValueObjects.Ids.GuidV7;

namespace ConferenceExample.Talk.Application.UnitTests;

public class SubmitTalkCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesTalkAndSaves()
    {
        // Arrange
        var talkRepository = Substitute.For<ITalkRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var userId = new UserId(AuthGuidV7.NewGuid());
        currentUserService.GetCurrentUserId().Returns(userId);

        var handler = new SubmitTalkCommandHandler(talkRepository, currentUserService);
        var command = new SubmitTalkCommand(
            "Test Title",
            "Test Abstract",
            Guid.NewGuid(),
            new List<string> { "tag1", "tag2" },
            Guid.NewGuid()
        );

        // Act
        await handler.Handle(command);

        // Assert
        await talkRepository
            .Received(1)
            .Save(Arg.Is<Domain.TalkManagement.Talk>(t => t.Title.Title == command.Title));
    }

    [Fact]
    public async Task Handle_ValidCommand_UsesCurrentUserIdAsSpeakerId()
    {
        // Arrange
        var talkRepository = Substitute.For<ITalkRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var userId = new UserId(AuthGuidV7.NewGuid());
        currentUserService.GetCurrentUserId().Returns(userId);

        var handler = new SubmitTalkCommandHandler(talkRepository, currentUserService);
        var command = new SubmitTalkCommand(
            "Test Title",
            "Test Abstract",
            Guid.NewGuid(),
            new List<string> { "tag1" },
            Guid.NewGuid()
        );

        // Act
        await handler.Handle(command);

        // Assert
        await talkRepository
            .Received(1)
            .Save(
                Arg.Is<Domain.TalkManagement.Talk>(t =>
                    t.SpeakerId == new SpeakerId(new GuidV7(userId.Value.Value))
                )
            );
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesWithCorrectProperties()
    {
        // Arrange
        var talkRepository = Substitute.For<ITalkRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var userId = new UserId(AuthGuidV7.NewGuid());
        currentUserService.GetCurrentUserId().Returns(userId);

        var handler = new SubmitTalkCommandHandler(talkRepository, currentUserService);
        var conferenceId = Guid.NewGuid();
        var talkTypeId = Guid.NewGuid();
        var command = new SubmitTalkCommand(
            "Test Title",
            "Test Abstract",
            conferenceId,
            new List<string> { "tag1", "tag2" },
            talkTypeId
        );

        // Act
        await handler.Handle(command);

        // Assert
        await talkRepository
            .Received(1)
            .Save(
                Arg.Is<Domain.TalkManagement.Talk>(t =>
                    t.Title.Title == command.Title
                    && t.Abstract.Content == command.Abstract
                    && t.ConferenceId == new ConferenceId(new GuidV7(conferenceId))
                    && t.TalkTypeId == new TalkTypeId(new GuidV7(talkTypeId))
                    && t.Tags.Count == 2
                )
            );
    }
}
