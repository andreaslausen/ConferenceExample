using ConferenceExample.Talk.Domain.SharedKernel;

namespace ConferenceExample.Talk.Domain.TalkManagement.Events;

public record TalkDeletedEvent(Guid AggregateId, DateTimeOffset OccurredAt) : IDomainEvent;
