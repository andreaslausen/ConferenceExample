using ConferenceExample.Persistence;
using ConferenceExample.Session.Domain.Repositories;
using ConferenceExample.Session.Domain.ValueObjects.Ids;
using ConferenceExample.Session.Persistence.Extensions;

namespace ConferenceExample.Session.Persistence;

public class SessionRepository(IDatabaseContext databaseContext) : ISessionRepository
{
    public Task<IReadOnlyList<Domain.Entities.Session>> GetSessions(ConferenceId conferenceId)
    {
        return Task.FromResult<IReadOnlyList<Domain.Entities.Session>>(
            databaseContext.Sessions.Where(s => s.ConferenceId == conferenceId.Value).Select(s => s.ToDomain()).ToList());
    }

    public Task Save(Domain.Entities.Session session)
    {
        databaseContext.Sessions.Add(session.ToPersistence());
        return Task.CompletedTask;
    }
}