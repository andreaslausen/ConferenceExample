using System.Text.Json;
using ConferenceExample.EventStore;
using ConferenceExample.Talk.Persistence.ReadModels;

namespace ConferenceExample.Talk.Persistence.EventHandlers;

/// <summary>
/// Projects the Conference BC's ConferenceCreatedEvent into a local read model mapping
/// ConferenceId to OrganizerId, so the Talk BC can authorize organizer access to talks
/// without depending on the Conference BC directly.
/// </summary>
public class ConferenceOrganizerEventHandler(IConferenceOrganizerDocumentRepository repository)
{
    public async Task HandleConferenceCreated(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<ConferenceCreatedPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        await repository.Save(
            new ConferenceOrganizerDocument
            {
                Id = storedEvent.AggregateId.ToString(),
                OrganizerId = payload.OrganizerId.ToString(),
            }
        );
    }

    private record ConferenceCreatedPayload(Guid OrganizerId);
}
