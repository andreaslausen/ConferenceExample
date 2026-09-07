namespace ConferenceExample.Talk.Persistence.ReadModels;

public interface ISpeakerDirectoryDocumentRepository
{
    Task Save(SpeakerDirectoryDocument document);
}
