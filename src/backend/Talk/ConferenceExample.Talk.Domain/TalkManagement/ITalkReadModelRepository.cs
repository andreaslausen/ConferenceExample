using ConferenceExample.Talk.Domain.SpeakerManagement;

namespace ConferenceExample.Talk.Domain.TalkManagement;

public interface ITalkReadModelRepository
{
    Task<IReadOnlyList<TalkReadModel>> GetBySpeakerId(SpeakerId speakerId);
}
