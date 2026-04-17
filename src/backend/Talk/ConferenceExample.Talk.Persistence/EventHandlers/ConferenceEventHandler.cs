using System.Text.Json;
using ConferenceExample.EventStore;
using ConferenceExample.Talk.Persistence.ReadModels;

namespace ConferenceExample.Talk.Persistence.EventHandlers;

/// <summary>
/// Event handler that synchronizes Conference events from the Conference BC to the Talk BC.
/// Updates the Conference Read Models when Conference domain events occur.
/// This enables the Talk BC to validate conference status without tight coupling.
/// </summary>
public class ConferenceEventHandler
{
    private readonly MongoDbConferenceReadModelRepository _readModelRepository;

    public ConferenceEventHandler(MongoDbConferenceReadModelRepository readModelRepository)
    {
        _readModelRepository = readModelRepository;
    }

    /// <summary>
    /// Handles any Conference domain event (created, renamed, status changed).
    /// All domain events contain the complete aggregate state, so we can use a single handler.
    /// </summary>
    public async Task HandleConferenceDomainEvent(StoredEvent storedEvent)
    {
        // All Conference domain events have the same structure with complete aggregate state
        var domainEvent = JsonSerializer.Deserialize<ConferenceDomainEventPayload>(
            storedEvent.Payload
        );
        if (domainEvent is null)
            return;

        // Check if read model already exists
        var existingReadModel = await _readModelRepository.GetReadModelById(
            storedEvent.AggregateId
        );

        if (existingReadModel is null)
        {
            // Create new read model with essential state from domain event
            var newReadModel = new ConferenceReadModel
            {
                Id = domainEvent.AggregateId.ToString(),
                Name = domainEvent.Name,
                Status = domainEvent.Status,
                CreatedAt = domainEvent.OccurredAt,
                LastModifiedAt = domainEvent.OccurredAt,
                Version = domainEvent.Version,
            };

            await _readModelRepository.Save(newReadModel);
        }
        else
        {
            // Update existing read model with state from domain event
            existingReadModel.Name = domainEvent.Name;
            existingReadModel.Status = domainEvent.Status;
            existingReadModel.LastModifiedAt = domainEvent.OccurredAt;
            existingReadModel.Version = domainEvent.Version;

            // Repository uses optimistic locking - will only update if version is newer
            await _readModelRepository.Update(existingReadModel);
        }
    }

    // DTO for deserializing any Conference domain event (all have the same structure now)
    private record ConferenceDomainEventPayload(
        Guid AggregateId,
        DateTimeOffset OccurredAt,
        long Version,
        string Name,
        DateTimeOffset Start,
        DateTimeOffset End,
        string LocationName,
        string Street,
        string City,
        string State,
        string PostalCode,
        string Country,
        Guid OrganizerId,
        string Status
    );
}
