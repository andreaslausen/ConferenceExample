namespace ConferenceExample.Conference.Persistence.ReadModels;

public interface IConferenceTalkDocumentRepository
{
    Task<ConferenceTalkDocument?> Get(Guid conferenceId, Guid talkId);
    Task<IReadOnlyList<ConferenceTalkDocument>> GetByConferenceId(Guid conferenceId);
    Task Save(ConferenceTalkDocument talkDocument);
    Task Update(ConferenceTalkDocument talkDocument);
}
