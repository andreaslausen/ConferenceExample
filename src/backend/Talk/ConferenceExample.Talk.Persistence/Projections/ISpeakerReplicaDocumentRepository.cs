namespace ConferenceExample.Talk.Persistence.Projections;

public interface ISpeakerReplicaDocumentRepository
{
    Task Save(SpeakerReplicaDocument document);
}
