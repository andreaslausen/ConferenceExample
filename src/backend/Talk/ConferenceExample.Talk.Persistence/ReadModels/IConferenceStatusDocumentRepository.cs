namespace ConferenceExample.Talk.Persistence.ReadModels;

public interface IConferenceStatusDocumentRepository
{
    Task Save(ConferenceStatusDocument document);

    Task<ConferenceStatusDocument?> GetById(string conferenceId);
}
