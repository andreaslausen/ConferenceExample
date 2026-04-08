using ConferenceExample.Session.Domain.ValueObjects.Ids;

namespace ConferenceExample.Session.Domain.Repositories;

public interface ISessionRepository
{
    Task<Entities.Session> GetById(SessionId id);
    Task<IReadOnlyList<Entities.Session>> GetSessions(ConferenceId conferenceId);
    Task Save(Entities.Session session);
}
