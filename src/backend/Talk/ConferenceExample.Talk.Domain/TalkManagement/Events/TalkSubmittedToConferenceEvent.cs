using ConferenceExample.Talk.Domain.SharedKernel;

namespace ConferenceExample.Talk.Domain.TalkManagement.Events;

/// <summary>
/// The speaker submits this talk to a conference. Carries the talk's content as a snapshot so both
/// the talk's own submission history and the receiving conference keep what was submitted, even
/// after the talk is edited or deleted.
///
/// The speaker's name and biography are deliberately not part of this payload: the Conference BC
/// snapshots those from its own speaker projection when it registers the submission, so the Talk
/// BC never has to read another context's profile data to raise one of its own events.
/// </summary>
public record TalkSubmittedToConferenceEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    Guid ConferenceId,
    Guid TalkTypeId,
    Guid SpeakerId,
    string Title,
    string Abstract,
    List<string> Tags
) : IDomainEvent;
