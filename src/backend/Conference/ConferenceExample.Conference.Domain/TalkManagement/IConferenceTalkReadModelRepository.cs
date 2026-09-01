using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;

namespace ConferenceExample.Conference.Domain.TalkManagement;

public interface IConferenceTalkReadModelRepository
{
    Task<IReadOnlyList<ConferenceTalkReadModel>> GetByConferenceId(ConferenceId conferenceId);
    Task<(IReadOnlyList<ConferenceTalkReadModel> Items, int TotalCount)> GetByConferenceId(
        ConferenceId conferenceId,
        PageRequest pageRequest
    );
}
