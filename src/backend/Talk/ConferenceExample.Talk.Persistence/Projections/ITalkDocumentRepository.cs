namespace ConferenceExample.Talk.Persistence.Projections;

public interface ITalkDocumentRepository
{
    Task<TalkDocument?> GetById(Guid talkId);
    Task Save(TalkDocument talkDocument);
    Task Update(TalkDocument talkDocument);
    Task Delete(Guid talkId);
}
