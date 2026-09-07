using System.Text.Json;
using ConferenceExample.EventStore;
using ConferenceExample.Talk.Persistence.ReadModels;

namespace ConferenceExample.Talk.Persistence.EventHandlers;

/// <summary>
/// Keeps conference names available for a speaker's submission list. Display data only — see
/// <see cref="Domain.ConferenceManagement.IConferenceDirectory"/>.
/// </summary>
public class ConferenceDirectoryEventHandler(IConferenceDocumentRepository repository)
{
    public async Task HandleConferenceCreated(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<ConferenceNamePayload>(storedEvent.Payload);
        if (payload is null)
            return;

        await repository.Save(
            new ConferenceDocument { Id = storedEvent.AggregateId.ToString(), Name = payload.Name }
        );
    }

    public async Task HandleConferenceRenamed(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<ConferenceNamePayload>(storedEvent.Payload);
        if (payload is null)
            return;

        await repository.UpdateName(storedEvent.AggregateId, payload.Name);
    }

    private record ConferenceNamePayload(string Name);
}
