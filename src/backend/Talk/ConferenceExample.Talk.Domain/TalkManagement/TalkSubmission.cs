using ConferenceExample.Talk.Domain.ConferenceManagement;

namespace ConferenceExample.Talk.Domain.TalkManagement;

/// <summary>
/// One submission of this talk to one conference, as the talk itself remembers it.
///
/// The content is a snapshot frozen at submission time: editing the talk afterwards changes future
/// submissions, never this one. The submission's outcome (accepted, rejected) is the conference's
/// decision and therefore not part of this aggregate — it reaches the speaker through the
/// submission read model, projected from the Conference BC's events.
/// </summary>
public record TalkSubmission(
    ConferenceId ConferenceId,
    TalkTypeId TalkTypeId,
    DateTimeOffset SubmittedAt,
    TalkTitle Title,
    Abstract Abstract,
    IReadOnlyList<TalkTag> Tags
);
