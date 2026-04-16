namespace ConferenceExample.Conference.Persistence.ReadModels;

/// <summary>
/// Repository interface for Conference Read Models in the Conference bounded context.
/// </summary>
public interface IConferenceReadModelRepository
{
    Task<ConferenceReadModel?> GetById(Guid conferenceId);
    Task<IReadOnlyList<ConferenceReadModel>> GetAll();
    Task Save(ConferenceReadModel conferenceReadModel);
    Task Update(ConferenceReadModel conferenceReadModel);
    Task Delete(Guid conferenceId);
}
