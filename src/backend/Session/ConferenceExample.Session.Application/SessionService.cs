using ConferenceExample.Session.Application.Commands;
using ConferenceExample.Session.Application.Dtos;

namespace ConferenceExample.Session.Application;

public class SessionService(ISubmitSessionCommandHandler submitSessionCommandHandler)
    : ISessionService
{
    public async Task SubmitSession(SubmitSessionDto submitSessionDto)
    {
        var command = new SubmitSessionCommand(
            submitSessionDto.Title,
            submitSessionDto.Abstract,
            submitSessionDto.ConferenceId,
            submitSessionDto.SpeakerId,
            submitSessionDto.Tags,
            submitSessionDto.SessionTypeId
        );

        await submitSessionCommandHandler.Handle(command);
    }
}
