namespace ConferenceExample.Session.Domain;

public interface IDomainEvent
{
    Guid AggregateId { get; }
    DateTimeOffset OccurredAt { get; }
}
