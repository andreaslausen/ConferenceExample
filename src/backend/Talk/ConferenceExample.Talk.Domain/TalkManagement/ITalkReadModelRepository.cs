using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.ValueObjects;

namespace ConferenceExample.Talk.Domain.TalkManagement;

public interface ITalkReadModelRepository
{
    Task<TalkReadModel?> GetById(TalkId talkId);

    Task<(IReadOnlyList<TalkReadModel> Items, int TotalCount)> GetBySpeakerId(
        SpeakerId speakerId,
        PageRequest pageRequest
    );
}
