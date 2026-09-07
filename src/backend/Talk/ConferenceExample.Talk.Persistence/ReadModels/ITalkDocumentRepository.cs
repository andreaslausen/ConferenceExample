namespace ConferenceExample.Talk.Persistence.ReadModels;

public interface ITalkDocumentRepository
{
    Task<TalkDocument?> GetById(Guid talkId);
    Task Save(TalkDocument talkDocument);
    Task Update(TalkDocument talkDocument);
    Task Delete(Guid talkId);
}
