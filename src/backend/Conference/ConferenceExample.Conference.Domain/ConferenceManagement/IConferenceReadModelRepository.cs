namespace ConferenceExample.Conference.Domain.ConferenceManagement;

public interface IConferenceReadModelRepository
{
    Task<IReadOnlyList<ConferenceReadModel>> GetAll();
}
