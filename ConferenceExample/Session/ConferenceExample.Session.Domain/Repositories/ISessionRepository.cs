using ConferenceExample.Session.Domain.ValueObjects.Ids;

namespace ConferenceExample.Session.Domain.Repositories;

public interface ISessionRepository
{
    public Task<IReadOnlyList<Entities.Session>> GetSessions(ConferenceId conferenceId);
    public Task Save(Entities.Session session);
}