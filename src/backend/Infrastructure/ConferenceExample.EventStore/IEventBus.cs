namespace ConferenceExample.EventStore;

public interface IEventBus
{
    void Subscribe(string eventType, Func<StoredEvent, Task> handler);

    void Publish(StoredEvent storedEvent);
}
