namespace ConferenceExample.Speaker.Persistence.ReadModels;

public interface ISpeakerDocumentRepository
{
    Task<SpeakerDocument?> GetById(Guid speakerId);

    Task Save(SpeakerDocument document);

    Task Update(SpeakerDocument document);
}
