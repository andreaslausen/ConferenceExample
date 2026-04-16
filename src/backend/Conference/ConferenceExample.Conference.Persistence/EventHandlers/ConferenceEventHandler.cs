using System.Text.Json;
using ConferenceExample.Conference.Persistence.ReadModels;
using ConferenceExample.EventStore;

namespace ConferenceExample.Conference.Persistence.EventHandlers;

/// <summary>
/// Event handler that updates Conference Read Models when any Conference domain event occurs.
/// Handles fat domain events that contain the complete aggregate state.
/// Uses optimistic locking via version checking in the repository.
/// </summary>
public class ConferenceEventHandler
{
    private readonly IConferenceReadModelRepository _readModelRepository;

    public ConferenceEventHandler(IConferenceReadModelRepository readModelRepository)
    {
        _readModelRepository = readModelRepository;
    }

    /// <summary>
    /// Handles any Conference domain event (created, renamed, status changed).
    /// All domain events now contain the complete aggregate state, so we can use a single handler.
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
        var existingReadModel = await _readModelRepository.GetById(storedEvent.AggregateId);

        if (existingReadModel is null)
        {
            // Create new read model with complete state from domain event
            var newReadModel = new ConferenceReadModel
            {
                Id = domainEvent.AggregateId.ToString(),
                Name = domainEvent.Name,
                Start = domainEvent.Start,
                End = domainEvent.End,
                LocationName = domainEvent.LocationName,
                Street = domainEvent.Street,
                City = domainEvent.City,
                State = domainEvent.State,
                PostalCode = domainEvent.PostalCode,
                Country = domainEvent.Country,
                OrganizerId = domainEvent.OrganizerId.ToString(),
                Status = domainEvent.Status,
                CreatedAt = domainEvent.OccurredAt,
                LastModifiedAt = domainEvent.OccurredAt,
                Version = domainEvent.Version,
            };

            await _readModelRepository.Save(newReadModel);
        }
        else
        {
            // Update existing read model with complete state from domain event
            existingReadModel.Name = domainEvent.Name;
            existingReadModel.Start = domainEvent.Start;
            existingReadModel.End = domainEvent.End;
            existingReadModel.LocationName = domainEvent.LocationName;
            existingReadModel.Street = domainEvent.Street;
            existingReadModel.City = domainEvent.City;
            existingReadModel.State = domainEvent.State;
            existingReadModel.PostalCode = domainEvent.PostalCode;
            existingReadModel.Country = domainEvent.Country;
            existingReadModel.OrganizerId = domainEvent.OrganizerId.ToString();
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
