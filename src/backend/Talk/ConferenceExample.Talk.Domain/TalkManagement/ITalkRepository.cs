namespace ConferenceExample.Talk.Domain.TalkManagement;

public interface ITalkRepository
{
    Task<Talk> GetById(TalkId id);
    Task<IReadOnlyList<Talk>> GetTalks(ConferenceId conferenceId);
    Task Save(Talk talk);
}
