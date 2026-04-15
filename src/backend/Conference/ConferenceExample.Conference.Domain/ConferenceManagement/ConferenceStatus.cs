namespace ConferenceExample.Conference.Domain.ConferenceManagement;

/// <summary>
/// Represents the lifecycle status of a conference.
/// </summary>
public enum ConferenceStatus
{
    /// <summary>
    /// Conference is in draft mode and not visible to speakers.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Call for Speakers is open - speakers can submit talks.
    /// </summary>
    CallForSpeakers = 1,

    /// <summary>
    /// Call for Speakers has closed - selection process is ongoing.
    /// </summary>
    CallForSpeakersClosed = 2,

    /// <summary>
    /// Conference program has been published.
    /// </summary>
    ProgramPublished = 3,
}
