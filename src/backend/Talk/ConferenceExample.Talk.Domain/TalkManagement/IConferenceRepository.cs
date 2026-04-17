namespace ConferenceExample.Talk.Domain.TalkManagement;

/// <summary>
/// Repository for loading Conference state from the Event Store.
/// Returns minimal Conference objects reconstructed from events.
/// This is used by command handlers to validate conference state before executing commands.
/// </summary>
public interface IConferenceRepository
{
    Task<Conference> GetById(ConferenceId conferenceId);
}
