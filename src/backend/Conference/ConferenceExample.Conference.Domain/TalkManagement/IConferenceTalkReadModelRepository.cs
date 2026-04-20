using ConferenceExample.Conference.Domain.ConferenceManagement;

namespace ConferenceExample.Conference.Domain.TalkManagement;

public interface IConferenceTalkReadModelRepository
{
    Task<IReadOnlyList<ConferenceTalkReadModel>> GetByConferenceId(ConferenceId conferenceId);
}
