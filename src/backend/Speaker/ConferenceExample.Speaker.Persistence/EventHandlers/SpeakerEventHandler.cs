using System.Text.Json;
using ConferenceExample.EventStore;
using ConferenceExample.Speaker.Persistence.ReadModels;

namespace ConferenceExample.Speaker.Persistence.EventHandlers;

public class SpeakerEventHandler(ISpeakerDocumentRepository readModelRepository)
{
    public async Task HandleSpeakerProfileCreated(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<SpeakerProfileCreatedPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var newDocument = new SpeakerDocument
        {
            Id = storedEvent.AggregateId.ToString(),
            UserId = payload.UserId.ToString(),
            FirstName = payload.FirstName,
            LastName = payload.LastName,
            Biography = payload.Biography,
            CreatedAt = storedEvent.OccurredAt,
            LastModifiedAt = storedEvent.OccurredAt,
            Version = storedEvent.Version,
        };

        await readModelRepository.Save(newDocument);
    }

    public async Task HandleSpeakerProfileUpdated(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<SpeakerProfileUpdatedPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var existingDocument = await readModelRepository.GetById(storedEvent.AggregateId);
        if (existingDocument is null)
            return;

        existingDocument.FirstName = payload.FirstName;
        existingDocument.LastName = payload.LastName;
        existingDocument.Biography = payload.Biography;
        existingDocument.LastModifiedAt = storedEvent.OccurredAt;
        existingDocument.Version = storedEvent.Version;

        await readModelRepository.Update(existingDocument);
    }

    private record SpeakerProfileCreatedPayload(
        Guid UserId,
        string FirstName,
        string LastName,
        string Biography
    );

    private record SpeakerProfileUpdatedPayload(
        string FirstName,
        string LastName,
        string Biography
    );
}
