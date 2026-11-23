namespace ConferenceExample.Conference.Persistence.ReadModels;

public interface IConferenceDocumentRepository
{
    Task<ConferenceDocument?> GetById(Guid conferenceId);
    Task<IReadOnlyList<ConferenceDocument>> GetAll();
    Task Save(ConferenceDocument conferenceDocument);
    Task Update(ConferenceDocument conferenceDocument);
    Task Delete(Guid conferenceId);
}
