using System.Text.Json;
using ConferenceExample.Conference.Persistence.ReadModels;
using ConferenceExample.EventStore;

namespace ConferenceExample.Conference.Persistence.EventHandlers;

/// <summary>
/// Keeps the Conference BC's copy of speaker profiles current, so a submission can be stamped with
/// who the speaker was at the moment it was registered.
/// </summary>
public class SpeakerDirectoryEventHandler(ISpeakerDocumentRepository repository)
{
    public async Task HandleSpeakerProfileChanged(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<SpeakerProfilePayload>(storedEvent.Payload);
        if (payload is null)
            return;

        await repository.Save(
            new SpeakerDocument
            {
                Id = storedEvent.AggregateId.ToString(),
                FirstName = payload.FirstName,
                LastName = payload.LastName,
                Biography = payload.Biography,
            }
        );
    }

    private record SpeakerProfilePayload(string FirstName, string LastName, string Biography);
}
