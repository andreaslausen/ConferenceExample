namespace ConferenceExample.Talk.Persistence.ReadModels;

public interface ITalkDocumentRepository
{
    Task<TalkDocument?> GetById(Guid talkId);
    Task<IReadOnlyList<TalkDocument>> GetByConferenceId(Guid conferenceId);
    Task<IReadOnlyList<TalkDocument>> GetBySpeakerId(Guid speakerId);
    Task Save(TalkDocument talkDocument);
    Task Update(TalkDocument talkDocument);
    Task Delete(Guid talkId);
}
