namespace ConferenceExample.Talk.Domain.TalkManagement;

public interface ITalkSubmissionReadModelRepository
{
    Task<IReadOnlyList<TalkSubmissionReadModel>> GetByTalkId(TalkId talkId);
}
