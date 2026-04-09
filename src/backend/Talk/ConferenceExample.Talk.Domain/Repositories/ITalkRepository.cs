using ConferenceExample.Talk.Domain.ValueObjects.Ids;

namespace ConferenceExample.Talk.Domain.Repositories;

public interface ITalkRepository
{
    Task<Entities.Talk> GetById(TalkId id);
    Task<IReadOnlyList<Entities.Talk>> GetTalks(ConferenceId conferenceId);
    Task Save(Entities.Talk talk);
}
