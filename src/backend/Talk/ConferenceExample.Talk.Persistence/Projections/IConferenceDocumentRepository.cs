namespace ConferenceExample.Talk.Persistence.Projections;

public interface IConferenceDocumentRepository
{
    Task Save(ConferenceDocument document);
    Task UpdateName(Guid conferenceId, string name);
}
