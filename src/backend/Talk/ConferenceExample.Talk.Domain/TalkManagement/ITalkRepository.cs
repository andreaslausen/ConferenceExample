namespace ConferenceExample.Talk.Domain.TalkManagement;

public interface ITalkRepository
{
    Task<Talk> GetById(TalkId id);
    Task Save(Talk talk);
}
