using ConferenceExample.Talk.Domain.SpeakerManagement;

namespace ConferenceExample.Talk.Domain.TalkManagement;

public interface ITalkReadModelRepository
{
    Task<TalkReadModel?> GetById(TalkId talkId);
    Task<IReadOnlyList<TalkReadModel>> GetBySpeakerId(SpeakerId speakerId);
}
