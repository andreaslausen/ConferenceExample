using ConferenceExample.Conference.Domain.Entities;
using ConferenceExample.Conference.Domain.ValueObjects.Ids;

namespace ConferenceExample.Conference.Domain.Repositories;

public interface ISessionRepository
{
    public Task<IReadOnlyList<Session>> GetSessions(ConferenceId conferenceId);
    public Task Save(Session session);
}