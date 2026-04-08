using ConferenceExample.Conference.Domain.ValueObjects.Ids;

namespace ConferenceExample.Conference.Domain.Repositories;

public interface IConferenceRepository
{
    Task<Conference> GetById(ConferenceId id);
    Task Save(Conference conference);
}
