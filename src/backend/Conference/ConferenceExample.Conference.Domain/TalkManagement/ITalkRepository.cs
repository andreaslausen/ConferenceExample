namespace ConferenceExample.Conference.Domain.TalkManagement;

/// <summary>
/// Repository for Talk Read Models in the Conference bounded context.
/// Currently reads from Event Store, later will read from dedicated Read Models.
/// </summary>
public interface ITalkRepository
{
    Task<IReadOnlyList<Talk>> GetTalksByConferenceId(
        ConferenceManagement.ConferenceId conferenceId
    );
}
