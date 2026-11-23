namespace ConferenceExample.Conference.Domain.SharedKernel;

public interface IDomainEvent
{
    Guid AggregateId { get; }
    DateTimeOffset OccurredAt { get; }
}
