namespace ConferenceExample.Conference.Persistence.ReadModels;

/// <summary>
/// Repository interface for Talk Read Models replicated in the Conference bounded context.
/// </summary>
public interface IConferenceTalkReadModelRepository
{
    Task<ConferenceTalkReadModel?> GetById(Guid talkId);
    Task<IReadOnlyList<ConferenceTalkReadModel>> GetByConferenceId(Guid conferenceId);
    Task Save(ConferenceTalkReadModel talkReadModel);
    Task Update(ConferenceTalkReadModel talkReadModel);
    Task Delete(Guid talkId);
}
