using ConferenceExample.Session.Application.Dtos;
using ConferenceExample.Session.Domain.Repositories;
using ConferenceExample.Session.Domain.ValueObjects;
using ConferenceExample.Session.Domain.ValueObjects.Ids;

namespace ConferenceExample.Session.Application;

public class SessionService(ISessionRepository sessionRepository, IDatabaseContext databaseContext) : ISessionService
{
    public async Task SubmitSession(SubmitSessionDto submitSessionDto)
    {
        var sessionId = new SessionId(GuidV7.NewGuid());
        var session = new Domain.Entities.Session(sessionId, new SessionTitle(submitSessionDto.Title), new SpeakerId(submitSessionDto.SpeakerId),
            submitSessionDto.Tags.Select(t => new SessionTag(t)), new SessionTypeId(submitSessionDto.SessionTypeId), new Abstract(submitSessionDto.Abstract),
            new ConferenceId(submitSessionDto.ConferenceId));

        await sessionRepository.Save(session);

        await databaseContext.SaveChanges();
    }
}