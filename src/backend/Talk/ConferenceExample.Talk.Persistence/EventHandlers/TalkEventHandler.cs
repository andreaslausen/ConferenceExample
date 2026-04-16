using System.Text.Json;
using ConferenceExample.EventStore;
using ConferenceExample.Talk.Persistence.ReadModels;

namespace ConferenceExample.Talk.Persistence.EventHandlers;

/// <summary>
/// Event handler that updates Talk Read Models when any Talk domain event occurs.
/// Handles fat domain events that contain the complete aggregate state.
/// Uses optimistic locking via version checking in the repository.
/// </summary>
public class TalkEventHandler
{
    private readonly ITalkReadModelRepository _readModelRepository;

    public TalkEventHandler(ITalkReadModelRepository readModelRepository)
    {
        _readModelRepository = readModelRepository;
    }

    /// <summary>
    /// Handles any Talk domain event (submitted, edited, tag added/removed).
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
            var newReadModel = new TalkReadModel
            {
                Id = domainEvent.AggregateId.ToString(),
                Title = domainEvent.Title,
                Abstract = domainEvent.Abstract,
                SpeakerId = domainEvent.SpeakerId.ToString(),
                TalkTypeId = domainEvent.TalkTypeId.ToString(),
                ConferenceId = domainEvent.ConferenceId.ToString(),
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
            existingReadModel.Title = domainEvent.Title;
            existingReadModel.Abstract = domainEvent.Abstract;
            existingReadModel.SpeakerId = domainEvent.SpeakerId.ToString();
            existingReadModel.TalkTypeId = domainEvent.TalkTypeId.ToString();
            existingReadModel.ConferenceId = domainEvent.ConferenceId.ToString();
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
}
