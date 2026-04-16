using System.Text.Json;
using ConferenceExample.Conference.Persistence.ReadModels;
using ConferenceExample.EventStore;

namespace ConferenceExample.Conference.Persistence.EventHandlers;

/// <summary>
/// Event handler that synchronizes Talk events from the Talk BC to the Conference BC.
/// Updates the Conference Talk Read Models when Talk domain events occur.
/// This enables the Conference BC to have denormalized talk data without tight coupling.
/// </summary>
public class TalkEventHandler
{
    private readonly IConferenceTalkReadModelRepository _readModelRepository;

    public TalkEventHandler(IConferenceTalkReadModelRepository readModelRepository)
    {
        _readModelRepository = readModelRepository;
    }

    /// <summary>
    /// Handles any Talk domain event from the Talk BC (submitted, edited, tag added/removed).
    /// All domain events now contain the complete aggregate state, so we can use a single handler.
    /// </summary>
    public async Task HandleTalkDomainEvent(StoredEvent storedEvent)
    {
        // All Talk domain events have the same structure with complete aggregate state
        var domainEvent = JsonSerializer.Deserialize<TalkDomainEventPayload>(storedEvent.Payload);
        if (domainEvent is null)
            return;

        // Check if read model already exists
        var existingReadModel = await _readModelRepository.GetById(storedEvent.AggregateId);

        if (existingReadModel is null)
        {
            // Create new read model with complete state from domain event
            var newReadModel = new ConferenceTalkReadModel
            {
                Id = domainEvent.AggregateId.ToString(),
                ConferenceId = domainEvent.ConferenceId.ToString(),
                Title = domainEvent.Title,
                Abstract = domainEvent.Abstract,
                SpeakerId = domainEvent.SpeakerId.ToString(),
                TalkTypeId = domainEvent.TalkTypeId.ToString(),
                Tags = domainEvent.Tags,
                Status = domainEvent.Status,
                SubmittedAt = domainEvent.OccurredAt,
                LastModifiedAt = domainEvent.OccurredAt,
                Version = domainEvent.Version,
            };

            await _readModelRepository.Save(newReadModel);
        }
        else
        {
            // Update existing read model with complete state from domain event
            // Preserve Conference BC specific fields (SlotStart, SlotEnd, RoomId, RoomName)
            existingReadModel.ConferenceId = domainEvent.ConferenceId.ToString();
            existingReadModel.Title = domainEvent.Title;
            existingReadModel.Abstract = domainEvent.Abstract;
            existingReadModel.SpeakerId = domainEvent.SpeakerId.ToString();
            existingReadModel.TalkTypeId = domainEvent.TalkTypeId.ToString();
            existingReadModel.Tags = domainEvent.Tags;
            existingReadModel.Status = domainEvent.Status;
            existingReadModel.LastModifiedAt = domainEvent.OccurredAt;
            existingReadModel.Version = domainEvent.Version;

            // Repository uses optimistic locking - will only update if version is newer
            await _readModelRepository.Update(existingReadModel);
        }
    }

    // DTO for deserializing any Talk domain event (all have the same structure now)
    private record TalkDomainEventPayload(
        Guid AggregateId,
        DateTimeOffset OccurredAt,
        long Version,
        string Title,
        string Abstract,
        Guid SpeakerId,
        List<string> Tags,
        Guid TalkTypeId,
        Guid ConferenceId,
        string Status
    );

    public async Task HandleTalkAccepted(StoredEvent storedEvent)
    {
        // Fat event contains full Conference state - we only care about TalkId for this handler
        var payload = JsonSerializer.Deserialize<TalkAcceptedPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await _readModelRepository.GetById(payload.TalkId);
        if (readModel is null)
            return;

        readModel.Status = "Accepted";
        readModel.LastModifiedAt = storedEvent.OccurredAt;

        await _readModelRepository.Update(readModel);
    }

    public async Task HandleTalkRejected(StoredEvent storedEvent)
    {
        // Fat event contains full Conference state - we only care about TalkId for this handler
        var payload = JsonSerializer.Deserialize<TalkRejectedPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await _readModelRepository.GetById(payload.TalkId);
        if (readModel is null)
            return;

        readModel.Status = "Rejected";
        readModel.LastModifiedAt = storedEvent.OccurredAt;

        await _readModelRepository.Update(readModel);
    }

    public async Task HandleTalkScheduled(StoredEvent storedEvent)
    {
        // Fat event contains full Conference state - extract TalkStart/TalkEnd for scheduling
        var payload = JsonSerializer.Deserialize<TalkScheduledPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await _readModelRepository.GetById(payload.TalkId);
        if (readModel is null)
            return;

        readModel.SlotStart = payload.TalkStart;
        readModel.SlotEnd = payload.TalkEnd;
        readModel.LastModifiedAt = storedEvent.OccurredAt;

        await _readModelRepository.Update(readModel);
    }

    public async Task HandleTalkAssignedToRoom(StoredEvent storedEvent)
    {
        // Fat event contains full Conference state - extract Room info
        var payload = JsonSerializer.Deserialize<TalkAssignedToRoomPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await _readModelRepository.GetById(payload.TalkId);
        if (readModel is null)
            return;

        readModel.RoomId = payload.RoomId.ToString();
        readModel.RoomName = payload.RoomName;
        readModel.LastModifiedAt = storedEvent.OccurredAt;

        await _readModelRepository.Update(readModel);
    }

    // Event payload DTOs (from Conference BC) - Fat events with full Conference state
    private record TalkAcceptedPayload(
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
        string Status,
        Guid TalkId
    );

    private record TalkRejectedPayload(
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
        string Status,
        Guid TalkId
    );

    private record TalkScheduledPayload(
        Guid AggregateId,
        DateTimeOffset OccurredAt,
        long Version,
        string Name,
        DateTimeOffset ConferenceStart,
        DateTimeOffset ConferenceEnd,
        string LocationName,
        string Street,
        string City,
        string State,
        string PostalCode,
        string Country,
        Guid OrganizerId,
        string Status,
        Guid TalkId,
        DateTimeOffset TalkStart,
        DateTimeOffset TalkEnd
    );

    private record TalkAssignedToRoomPayload(
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
        string Status,
        Guid TalkId,
        Guid RoomId,
        string RoomName
    );
}
