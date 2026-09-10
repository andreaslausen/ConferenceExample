namespace ConferenceExample.Conference.Persistence.ReadModels;

public interface ISpeakerDocumentRepository
{
    Task<SpeakerDocument?> GetById(Guid speakerId);
    Task Save(SpeakerDocument document);
}
