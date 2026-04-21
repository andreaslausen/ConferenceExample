using System.Text.Json;
using ConferenceExample.EventStore;
using ConferenceExample.Talk.Persistence.ReadModels;

namespace ConferenceExample.Talk.Persistence.EventHandlers;

public class SpeakerEventHandler(ISpeakerDocumentRepository readModelRepository)
{
    public async Task HandleSpeakerDomainEvent(StoredEvent storedEvent)
    {
        var domainEvent = JsonSerializer.Deserialize<SpeakerDomainEventPayload>(
            storedEvent.Payload
        );
        if (domainEvent is null)
            return;

        var existingDocument = await readModelRepository.GetById(storedEvent.AggregateId);

        if (existingDocument is null)
        {
            var newDocument = new SpeakerDocument
            {
                Id = domainEvent.AggregateId.ToString(),
                FirstName = domainEvent.FirstName,
                LastName = domainEvent.LastName,
                Biography = domainEvent.Biography,
                CreatedAt = domainEvent.OccurredAt,
                LastModifiedAt = domainEvent.OccurredAt,
                Version = storedEvent.Version,
            };

            await readModelRepository.Save(newDocument);
        }
        else
        {
            // Use the event store version (storedEvent.Version) for idempotency rather than the
            // embedded payload version, which can be identical across multiple events raised within
            // the same command (e.g. EditTalk).
            if (storedEvent.Version <= existingDocument.Version)
                return;

            existingDocument.FirstName = domainEvent.FirstName;
            existingDocument.LastName = domainEvent.LastName;
            existingDocument.Biography = domainEvent.Biography;
            existingDocument.LastModifiedAt = domainEvent.OccurredAt;
            existingDocument.Version = storedEvent.Version;

            await readModelRepository.Update(existingDocument);
        }
    }

    private record SpeakerDomainEventPayload(
        Guid AggregateId,
        DateTimeOffset OccurredAt,
        long Version,
        string FirstName,
        string LastName,
        string Biography
    );
}
