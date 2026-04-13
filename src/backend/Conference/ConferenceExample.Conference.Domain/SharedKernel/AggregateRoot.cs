namespace ConferenceExample.Conference.Domain.SharedKernel;

public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _uncommittedEvents = [];

    public long Version { get; protected set; } = -1;

    public IReadOnlyList<IDomainEvent> GetUncommittedEvents() => _uncommittedEvents;

    public void ClearUncommittedEvents() => _uncommittedEvents.Clear();

    protected void RaiseEvent(IDomainEvent @event)
    {
        ApplyEvent(@event);
        _uncommittedEvents.Add(@event);
    }

    public void ReplayEvents(IEnumerable<IDomainEvent> events)
    {
        foreach (var @event in events)
        {
            ApplyEvent(@event);
            Version++;
        }
    }

    protected abstract void ApplyEvent(IDomainEvent @event);
}
