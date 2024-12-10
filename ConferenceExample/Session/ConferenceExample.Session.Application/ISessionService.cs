using ConferenceExample.Session.Application.Dtos;

namespace ConferenceExample.Session.Application;

public interface ISessionService
{
    Task SubmitSession(SubmitSessionDto submitSessionDto);
}