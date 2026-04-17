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
        var conferenceRepository = Substitute.For<IConferenceRepository>();
        var userId = new UserId(AuthGuidV7.NewGuid());
        currentUserService.GetCurrentUserId().Returns(userId);

        var conferenceId = Guid.CreateVersion7();
        var conference = Domain.TalkManagement.Conference.FromEvents(
            new ConferenceId(new GuidV7(conferenceId)),
            "CallForSpeakers"
        );
        conferenceRepository.GetById(Arg.Any<ConferenceId>()).Returns(conference);

        var handler = new SubmitTalkCommandHandler(
            talkRepository,
            currentUserService,
            conferenceRepository
        );
        var command = new SubmitTalkCommand(
            "Test Title",
            "Test Abstract",
            conferenceId,
            ["tag1", "tag2"],
            Guid.CreateVersion7()
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
        var conferenceRepository = Substitute.For<IConferenceRepository>();
        var userId = new UserId(AuthGuidV7.NewGuid());
        currentUserService.GetCurrentUserId().Returns(userId);

        var conferenceId = Guid.CreateVersion7();
        var conference = Domain.TalkManagement.Conference.FromEvents(
            new ConferenceId(new GuidV7(conferenceId)),
            "CallForSpeakers"
        );
        conferenceRepository.GetById(Arg.Any<ConferenceId>()).Returns(conference);

        var handler = new SubmitTalkCommandHandler(
            talkRepository,
            currentUserService,
            conferenceRepository
        );
        var command = new SubmitTalkCommand(
            "Test Title",
            "Test Abstract",
            conferenceId,
            ["tag1"],
            Guid.CreateVersion7()
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
        var conferenceRepository = Substitute.For<IConferenceRepository>();
        var userId = new UserId(AuthGuidV7.NewGuid());
        currentUserService.GetCurrentUserId().Returns(userId);

        var conferenceId = Guid.CreateVersion7();
        var conference = Domain.TalkManagement.Conference.FromEvents(
            new ConferenceId(new GuidV7(conferenceId)),
            "CallForSpeakers"
        );
        conferenceRepository.GetById(Arg.Any<ConferenceId>()).Returns(conference);

        var handler = new SubmitTalkCommandHandler(
            talkRepository,
            currentUserService,
            conferenceRepository
        );
        var talkTypeId = Guid.CreateVersion7();
        var command = new SubmitTalkCommand(
            "Test Title",
            "Test Abstract",
            conferenceId,
            ["tag1", "tag2"],
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

    [Fact]
    public async Task Handle_ConferenceDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var talkRepository = Substitute.For<ITalkRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var conferenceRepository = Substitute.For<IConferenceRepository>();
        var userId = new UserId(AuthGuidV7.NewGuid());
        currentUserService.GetCurrentUserId().Returns(userId);

        var conferenceId = Guid.CreateVersion7();
        conferenceRepository
            .GetById(Arg.Any<ConferenceId>())
            .Returns(
                Task.FromException<Domain.TalkManagement.Conference>(
                    new InvalidOperationException("Conference with id does not exist.")
                )
            );

        var handler = new SubmitTalkCommandHandler(
            talkRepository,
            currentUserService,
            conferenceRepository
        );
        var command = new SubmitTalkCommand(
            "Test Title",
            "Test Abstract",
            conferenceId,
            ["tag1"],
            Guid.CreateVersion7()
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await handler.Handle(command)
        );
        Assert.Contains("does not exist", exception.Message);
    }

    [Fact]
    public async Task Handle_ConferenceStatusIsNotCallForSpeakers_ThrowsInvalidOperationException()
    {
        // Arrange
        var talkRepository = Substitute.For<ITalkRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var conferenceRepository = Substitute.For<IConferenceRepository>();
        var userId = new UserId(AuthGuidV7.NewGuid());
        currentUserService.GetCurrentUserId().Returns(userId);

        var conferenceId = Guid.CreateVersion7();
        var conference = Domain.TalkManagement.Conference.FromEvents(
            new ConferenceId(new GuidV7(conferenceId)),
            "Draft"
        );
        conferenceRepository.GetById(Arg.Any<ConferenceId>()).Returns(conference);

        var handler = new SubmitTalkCommandHandler(
            talkRepository,
            currentUserService,
            conferenceRepository
        );
        var command = new SubmitTalkCommand(
            "Test Title",
            "Test Abstract",
            conferenceId,
            ["tag1"],
            Guid.CreateVersion7()
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await handler.Handle(command)
        );
        Assert.Contains("CallForSpeakers", exception.Message);
    }

    [Fact]
    public async Task Handle_ConferenceStatusIsCallForSpeakersClosed_ThrowsInvalidOperationException()
    {
        // Arrange
        var talkRepository = Substitute.For<ITalkRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var conferenceRepository = Substitute.For<IConferenceRepository>();
        var userId = new UserId(AuthGuidV7.NewGuid());
        currentUserService.GetCurrentUserId().Returns(userId);

        var conferenceId = Guid.CreateVersion7();
        var conference = Domain.TalkManagement.Conference.FromEvents(
            new ConferenceId(new GuidV7(conferenceId)),
            "CallForSpeakersClosed"
        );
        conferenceRepository.GetById(Arg.Any<ConferenceId>()).Returns(conference);

        var handler = new SubmitTalkCommandHandler(
            talkRepository,
            currentUserService,
            conferenceRepository
        );
        var command = new SubmitTalkCommand(
            "Test Title",
            "Test Abstract",
            conferenceId,
            ["tag1"],
            Guid.CreateVersion7()
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await handler.Handle(command)
        );
        Assert.Contains("CallForSpeakers", exception.Message);
    }
}
