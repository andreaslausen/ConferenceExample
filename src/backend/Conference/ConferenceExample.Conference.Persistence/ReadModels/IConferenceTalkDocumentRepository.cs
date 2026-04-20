namespace ConferenceExample.Conference.Persistence.ReadModels;

public interface IConferenceTalkDocumentRepository
{
    Task<ConferenceTalkDocument?> GetById(Guid talkId);
    Task<IReadOnlyList<ConferenceTalkDocument>> GetByConferenceId(Guid conferenceId);
    Task Save(ConferenceTalkDocument talkDocument);
    Task Update(ConferenceTalkDocument talkDocument);
    Task Delete(Guid talkId);
}
