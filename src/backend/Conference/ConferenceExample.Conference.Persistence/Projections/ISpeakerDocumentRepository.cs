namespace ConferenceExample.Conference.Persistence.Projections;

public interface ISpeakerDocumentRepository
{
    Task<SpeakerDocument?> GetById(Guid speakerId);
    Task Save(SpeakerDocument document);
}
