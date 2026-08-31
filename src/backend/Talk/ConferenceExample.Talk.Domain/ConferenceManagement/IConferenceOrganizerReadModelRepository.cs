namespace ConferenceExample.Talk.Domain.ConferenceManagement;

public interface IConferenceOrganizerReadModelRepository
{
    Task<ConferenceOrganizerReadModel?> GetByConferenceId(Guid conferenceId);
}
