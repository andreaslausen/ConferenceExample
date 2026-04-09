namespace ConferenceExample.EventStore;

public interface IEventBus
{
    void Subscribe(string eventType, Action<StoredEvent> handler);
    Task Publish(IEnumerable<StoredEvent> events);
}
