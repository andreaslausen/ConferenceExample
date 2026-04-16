using System.Text.Json;
using ConferenceExample.Conference.Persistence.ReadModels;
using ConferenceExample.EventStore;

namespace ConferenceExample.Conference.Persistence.EventHandlers;

/// <summary>
/// Event handler that updates Conference Read Models when Conference domain events occur.
/// Subscribes to events from the EventBus and projects them into the Read Model store.
/// </summary>
public class ConferenceEventHandler
{
    private readonly IConferenceReadModelRepository _readModelRepository;

    public ConferenceEventHandler(IConferenceReadModelRepository readModelRepository)
    {
        _readModelRepository = readModelRepository;
    }

    public async Task HandleConferenceCreated(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<ConferenceCreatedPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = new ConferenceReadModel
        {
            Id = storedEvent.AggregateId.ToString(),
            Name = payload.Name,
            Start = payload.Start,
            End = payload.End,
            LocationName = payload.LocationName,
            Street = payload.Street,
            City = payload.City,
            State = payload.State,
            PostalCode = payload.PostalCode,
            Country = payload.Country,
            OrganizerId = payload.OrganizerId.ToString(),
            Status = "Draft",
            CreatedAt = storedEvent.OccurredAt,
            LastModifiedAt = storedEvent.OccurredAt,
        };

        await _readModelRepository.Save(readModel);
    }

    public async Task HandleConferenceRenamed(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<ConferenceRenamedPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await _readModelRepository.GetById(storedEvent.AggregateId);
        if (readModel is null)
            return;

        readModel.Name = payload.Name;
        readModel.LastModifiedAt = storedEvent.OccurredAt;

        await _readModelRepository.Update(readModel);
    }

    public async Task HandleConferenceStatusChanged(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<ConferenceStatusChangedPayload>(
            storedEvent.Payload
        );
        if (payload is null)
            return;

        var readModel = await _readModelRepository.GetById(storedEvent.AggregateId);
        if (readModel is null)
            return;

        readModel.Status = payload.NewStatus.ToString();
        readModel.LastModifiedAt = storedEvent.OccurredAt;

        await _readModelRepository.Update(readModel);
    }

    // Event payload DTOs
    private record ConferenceCreatedPayload(
        string Name,
        DateTimeOffset Start,
        DateTimeOffset End,
        string LocationName,
        string Street,
        string City,
        string State,
        string PostalCode,
        string Country,
        Guid OrganizerId
    );

    private record ConferenceRenamedPayload(string Name);

    private record ConferenceStatusChangedPayload(int NewStatus);
}
