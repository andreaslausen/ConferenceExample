namespace ConferenceExample.Talk.Domain;

public interface IDomainEvent
{
    Guid AggregateId { get; }
    DateTimeOffset OccurredAt { get; }
}
