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
