using ConferenceExample.Session.Application.Dtos;
using ConferenceExample.Session.Domain.Repositories;
using ConferenceExample.Session.Domain.ValueObjects;
using ConferenceExample.Session.Domain.ValueObjects.Ids;

namespace ConferenceExample.Session.Application;

public class SessionService(ISessionRepository sessionRepository) : ISessionService
{
    public async Task SubmitSession(SubmitSessionDto submitSessionDto)
    {
        var session = Domain.Entities.Session.Submit(
            new SessionId(GuidV7.NewGuid()),
            new SessionTitle(submitSessionDto.Title),
            new SpeakerId(submitSessionDto.SpeakerId),
            submitSessionDto.Tags.Select(t => new SessionTag(t)),
            new SessionTypeId(submitSessionDto.SessionTypeId),
            new Abstract(submitSessionDto.Abstract),
            new ConferenceId(submitSessionDto.ConferenceId)
        );

        await sessionRepository.Save(session);
    }
}
