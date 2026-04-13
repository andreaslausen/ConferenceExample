using ConferenceExample.Conference.Domain.SharedKernel;

namespace ConferenceExample.Conference.Domain.TalkManagement.Events;

public record TalkSubmittedToConferenceEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    Guid TalkId
) : IDomainEvent;
