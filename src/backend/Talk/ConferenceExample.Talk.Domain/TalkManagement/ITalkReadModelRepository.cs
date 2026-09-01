using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Talk.Domain.SpeakerManagement;

namespace ConferenceExample.Talk.Domain.TalkManagement;

public interface ITalkReadModelRepository
{
    Task<TalkReadModel?> GetById(TalkId talkId);
    Task<(IReadOnlyList<TalkReadModel> Items, int TotalCount)> GetBySpeakerId(
        SpeakerId speakerId,
        PageRequest pageRequest
    );
}
