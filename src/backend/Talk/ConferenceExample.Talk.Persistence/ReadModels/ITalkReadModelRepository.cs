namespace ConferenceExample.Talk.Persistence.ReadModels;

/// <summary>
/// Repository interface for Talk Read Models in the Talk bounded context.
/// </summary>
public interface ITalkReadModelRepository
{
    Task<TalkReadModel?> GetById(Guid talkId);
    Task<IReadOnlyList<TalkReadModel>> GetByConferenceId(Guid conferenceId);
    Task<IReadOnlyList<TalkReadModel>> GetBySpeakerId(Guid speakerId);
    Task Save(TalkReadModel talkReadModel);
    Task Update(TalkReadModel talkReadModel);
    Task Delete(Guid talkId);
}
