using ConferenceExample.Talk.Application.EditTalk;
using ConferenceExample.Talk.Application.GetMyTalks;
using ConferenceExample.Talk.Application.SubmitTalk;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using NSubstitute;

namespace ConferenceExample.Talk.Application.UnitTests;

public class TalkServiceTests
{
    [Fact]
    public async Task SubmitTalk_CallsCommandHandler_MethodCalled()
    {
        // Arrange
        var submitTalkCommandHandler = Substitute.For<ISubmitTalkCommandHandler>();
        var getMyTalksQueryHandler = Substitute.For<IGetMyTalksQueryHandler>();
        var editTalkCommandHandler = Substitute.For<IEditTalkCommandHandler>();
        var service = new TalkService(
            submitTalkCommandHandler,
            getMyTalksQueryHandler,
            editTalkCommandHandler
        );
        var dto = CreateSubmitTalkDto();

        // Act
        await service.SubmitTalk(dto);

        // Assert
        await submitTalkCommandHandler.Received(1).Handle(Arg.Any<SubmitTalkCommand>());
    }

    [Fact]
    public async Task GetMyTalks_CallsQueryHandler_ReturnsResult()
    {
        // Arrange
        var submitTalkCommandHandler = Substitute.For<ISubmitTalkCommandHandler>();
        var getMyTalksQueryHandler = Substitute.For<IGetMyTalksQueryHandler>();
        var editTalkCommandHandler = Substitute.For<IEditTalkCommandHandler>();
        var service = new TalkService(
            submitTalkCommandHandler,
            getMyTalksQueryHandler,
            editTalkCommandHandler
        );
        var expectedTalks = new List<GetMyTalksDto>
        {
            new(
                Guid.NewGuid(),
                "Title",
                "Abstract",
                Guid.NewGuid(),
                "Submitted",
                new List<string>()
            ),
        };
        getMyTalksQueryHandler.Handle(Arg.Any<GetMyTalksQuery>()).Returns(expectedTalks);

        // Act
        var result = await service.GetMyTalks();

        // Assert
        await getMyTalksQueryHandler.Received(1).Handle(Arg.Any<GetMyTalksQuery>());
        Assert.Equal(expectedTalks, result);
    }

    [Fact]
    public async Task EditTalk_CallsCommandHandler_MethodCalled()
    {
        // Arrange
        var submitTalkCommandHandler = Substitute.For<ISubmitTalkCommandHandler>();
        var getMyTalksQueryHandler = Substitute.For<IGetMyTalksQueryHandler>();
        var editTalkCommandHandler = Substitute.For<IEditTalkCommandHandler>();
        var service = new TalkService(
            submitTalkCommandHandler,
            getMyTalksQueryHandler,
            editTalkCommandHandler
        );
        var talkId = Guid.NewGuid();
        var dto = new EditTalkDto("Updated Title", "Updated Abstract", new List<string> { "tag1" });

        // Act
        await service.EditTalk(talkId, dto);

        // Assert
        await editTalkCommandHandler.Received(1).Handle(Arg.Any<EditTalkCommand>());
    }

    private SubmitTalkDto CreateSubmitTalkDto()
    {
        return new SubmitTalkDto
        {
            Title = "Test Title",
            Tags = new List<string> { "tag1", "tag2" },
            TalkTypeId = GuidV7.NewGuid(),
            Abstract = "Test Abstract",
            ConferenceId = GuidV7.NewGuid(),
        };
    }
}
