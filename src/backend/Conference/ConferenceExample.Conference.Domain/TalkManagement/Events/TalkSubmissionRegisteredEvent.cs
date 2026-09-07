using ConferenceExample.Conference.Domain.SharedKernel;

namespace ConferenceExample.Conference.Domain.TalkManagement.Events;

/// <summary>
/// The conference accepted a talk into its review queue. Answers the Talk BC's
/// <c>TalkSubmittedToConferenceEvent</c>; the counterpart for a submission that never gets that far
/// is <see cref="TalkSubmissionRejectedEvent"/>.
/// </summary>
public record TalkSubmissionRegisteredEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    Guid TalkId,
    Guid TalkTypeId
) : IDomainEvent;
