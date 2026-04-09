using ConferenceExample.Session.Application.Commands;
using ConferenceExample.Session.Application.Dtos;
using ConferenceExample.Session.Domain.ValueObjects;
using ConferenceExample.Session.Domain.ValueObjects.Ids;
using NSubstitute;

namespace ConferenceExample.Session.Application.UnitTests;

public class SessionServiceTests
{
    [Fact]
    public async Task SubmitSession_CallsCommandHandler_MethodCalled()
    {
        // Arrange
        var commandHandler = Substitute.For<ISubmitSessionCommandHandler>();
        var service = new SessionService(commandHandler);
        var dto = CreateSubmitSessionDto();

        // Act
        await service.SubmitSession(dto);

        // Assert
        await commandHandler.Received(1).Handle(Arg.Any<SubmitSessionCommand>());
    }

    private SubmitSessionDto CreateSubmitSessionDto()
    {
        return new SubmitSessionDto
        {
            Title = "Test Title",
            SpeakerId = GuidV7.NewGuid(),
            Tags = new List<string> { "tag1", "tag2" },
            SessionTypeId = GuidV7.NewGuid(),
            Abstract = "Test Abstract",
            ConferenceId = GuidV7.NewGuid(),
        };
    }
}
