namespace ConferenceExample.Talk.Domain.TalkManagement;

/// <summary>
/// Repository interface for querying conference information from the Talk bounded context.
/// Returns domain objects (ConferenceInfo) without any infrastructure dependencies.
/// Implemented in the Persistence layer using MongoDB read models.
/// </summary>
public interface IConferenceInfoRepository
{
    Task<ConferenceInfo?> GetById(ConferenceId conferenceId);
}
