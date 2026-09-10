using System.Text.Json;
using ConferenceExample.EventStore;
using ConferenceExample.Talk.Persistence.ReadModels;

namespace ConferenceExample.Talk.Persistence.EventHandlers;

/// <summary>
/// Projects the Speaker BC's SpeakerProfileCreatedEvent into the local UserId -> SpeakerId index,
/// so the Talk BC can tell whose talks these are without depending on the Speaker BC directly.
/// Profile content is deliberately not replicated: the Talk BC has no use for it.
/// </summary>
public class SpeakerDirectoryEventHandler(ISpeakerDirectoryDocumentRepository repository)
{
    public async Task HandleSpeakerProfileCreated(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<SpeakerProfileCreatedPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        await repository.Save(
            new SpeakerDirectoryDocument
            {
                Id = storedEvent.AggregateId.ToString(),
                UserId = payload.UserId.ToString(),
            }
        );
    }

    private record SpeakerProfileCreatedPayload(Guid UserId);
}
