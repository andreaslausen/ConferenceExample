namespace ConferenceExample.Talk.Persistence.Projections;

public interface ITalkSubmissionDocumentRepository
{
    Task<TalkSubmissionDocument?> Get(Guid talkId, Guid conferenceId);
    Task Save(TalkSubmissionDocument document);
    Task Update(TalkSubmissionDocument document);
}
