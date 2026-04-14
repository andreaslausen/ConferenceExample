using ConferenceExample.Talk.Domain.SpeakerManagement;

namespace ConferenceExample.Talk.Domain.TalkManagement;

public interface ITalkRepository
{
    Task<Talk> GetById(TalkId id);
    Task<IReadOnlyList<Talk>> GetTalks(ConferenceId conferenceId);
    Task<IReadOnlyList<Talk>> GetTalksBySpeaker(SpeakerId speakerId);
    Task Save(Talk talk);
}
