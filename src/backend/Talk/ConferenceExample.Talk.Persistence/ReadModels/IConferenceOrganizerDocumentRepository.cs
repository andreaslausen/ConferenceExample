namespace ConferenceExample.Talk.Persistence.ReadModels;

public interface IConferenceOrganizerDocumentRepository
{
    Task Save(ConferenceOrganizerDocument document);
}
