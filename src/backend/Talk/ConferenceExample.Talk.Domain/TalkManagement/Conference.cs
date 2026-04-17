using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Talk.Domain.TalkManagement;

/// <summary>
/// Minimal Conference representation in the Talk BC for validation purposes.
/// Contains only the information needed to validate talk submissions.
/// The full Conference aggregate lives in the Conference BC.
/// </summary>
public class Conference
{
    public ConferenceId Id { get; private set; } = null!;
    public string Status { get; private set; } = null!;

    private Conference() { }

    /// <summary>
    /// Factory method to reconstruct Conference from events.
    /// Only reconstructs the minimal state needed for validation.
    /// </summary>
    public static Conference FromEvents(ConferenceId id, string status)
    {
        return new Conference { Id = id, Status = status };
    }

    /// <summary>
    /// Validates if talks can be submitted to this conference.
    /// </summary>
    public bool CanAcceptTalkSubmissions()
    {
        return Status == "CallForSpeakers";
    }
}
