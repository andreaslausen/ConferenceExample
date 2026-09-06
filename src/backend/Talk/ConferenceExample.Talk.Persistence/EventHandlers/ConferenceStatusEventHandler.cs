using System.Text.Json;
using ConferenceExample.EventStore;
using ConferenceExample.Talk.Persistence.ReadModels;

namespace ConferenceExample.Talk.Persistence.EventHandlers;

/// <summary>
/// Projects the Conference BC's status-bearing events into a minimal local document, so
/// SubmitTalkCommandHandler can validate conference status via IConferenceRepository without
/// depending on Conference's full event schema or a query-only ReadModel.
/// </summary>
public class ConferenceStatusEventHandler(IConferenceStatusDocumentRepository repository)
{
    public Task HandleConferenceCreated(StoredEvent storedEvent) => SaveStatus(storedEvent);

    public Task HandleConferenceStatusChanged(StoredEvent storedEvent) => SaveStatus(storedEvent);

    private async Task SaveStatus(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<StatusPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        await repository.Save(
            new ConferenceStatusDocument
            {
                Id = storedEvent.AggregateId.ToString(),
                Status = payload.Status,
            }
        );
    }

    private record StatusPayload(string Status);
}
