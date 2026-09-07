namespace ConferenceExample.Talk.Domain.TalkManagement;

/// <summary>
/// Where one submission stands. Only <see cref="Pending"/> is set by the Talk BC itself; every
/// other value is projected from a decision the Conference BC made.
/// </summary>
public enum SubmissionStatus
{
    /// <summary>Submitted, but the conference has not confirmed receipt yet.</summary>
    Pending = 0,

    /// <summary>Registered by the conference and awaiting review.</summary>
    Submitted = 1,

    /// <summary>Accepted into the conference's program.</summary>
    Accepted = 2,

    /// <summary>Reviewed by the conference and turned down.</summary>
    Rejected = 3,

    /// <summary>
    /// Never made it into review — the conference does not exist, or was not accepting
    /// submissions. Carries a reason.
    /// </summary>
    Failed = 4,
}
