namespace ConferenceExample.Talk.Persistence.ReadModels;

public interface ITalkSubmissionDocumentRepository
{
    Task<TalkSubmissionDocument?> Get(Guid talkId, Guid conferenceId);
    Task Save(TalkSubmissionDocument document);
    Task Update(TalkSubmissionDocument document);
}
