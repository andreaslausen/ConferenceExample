using ConferenceExample.Conference.Domain;

namespace ConferenceExample.Conference.Domain.TalkManagement.Events;

public record TalkSubmissionRejectedEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    Guid TalkId,
    string Reason
) : IDomainEvent;
