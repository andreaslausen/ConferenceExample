namespace ConferenceExample.Talk.Persistence.ReadModels;

public interface IConferenceDocumentRepository
{
    Task Save(ConferenceDocument document);
    Task UpdateName(Guid conferenceId, string name);
}
