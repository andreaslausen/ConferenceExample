namespace ConferenceExample.Talk.Domain.ConferenceManagement;

/// <summary>
/// Names of the conferences a speaker submitted to, replicated from the Conference BC. Display
/// data only — the Talk BC never decides anything based on it, and never keeps a copy of a
/// conference's state.
/// </summary>
public interface IConferenceDirectory
{
    Task<IReadOnlyDictionary<Guid, string>> GetConferenceNames(
        IReadOnlyCollection<ConferenceId> conferenceIds
    );
}
