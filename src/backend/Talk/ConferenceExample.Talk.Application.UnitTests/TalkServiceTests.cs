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
        var commandHandler = Substitute.For<ISubmitTalkCommandHandler>();
        var service = new TalkService(commandHandler);
        var dto = CreateSubmitTalkDto();

        // Act
        await service.SubmitTalk(dto);

        // Assert
        await commandHandler.Received(1).Handle(Arg.Any<SubmitTalkCommand>());
    }

    private SubmitTalkDto CreateSubmitTalkDto()
    {
        return new SubmitTalkDto
        {
            Title = "Test Title",
            SpeakerId = GuidV7.NewGuid(),
            Tags = new List<string> { "tag1", "tag2" },
            TalkTypeId = GuidV7.NewGuid(),
            Abstract = "Test Abstract",
            ConferenceId = GuidV7.NewGuid(),
        };
    }
}
