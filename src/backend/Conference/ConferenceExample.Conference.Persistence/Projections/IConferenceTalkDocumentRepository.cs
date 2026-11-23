namespace ConferenceExample.Conference.Persistence.Projections;

public interface IConferenceTalkDocumentRepository
{
    Task<ConferenceTalkDocument?> Get(Guid conferenceId, Guid talkId);
    Task<IReadOnlyList<ConferenceTalkDocument>> GetByConferenceId(Guid conferenceId);
    Task Save(ConferenceTalkDocument talkDocument);
    Task Update(ConferenceTalkDocument talkDocument);
}
