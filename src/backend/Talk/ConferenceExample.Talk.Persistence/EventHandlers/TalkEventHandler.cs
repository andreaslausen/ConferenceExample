using System.Text.Json;
using ConferenceExample.EventStore;
using ConferenceExample.Talk.Persistence.ReadModels;

namespace ConferenceExample.Talk.Persistence.EventHandlers;

/// <summary>
/// Event handler that updates Talk Read Models when Talk domain events occur.
/// Subscribes to events from the EventBus and projects them into the Read Model store.
/// </summary>
public class TalkEventHandler
{
    private readonly ITalkReadModelRepository _readModelRepository;

    public TalkEventHandler(ITalkReadModelRepository readModelRepository)
    {
        _readModelRepository = readModelRepository;
    }

    public async Task HandleTalkSubmitted(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TalkSubmittedPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = new TalkReadModel
        {
            Id = storedEvent.AggregateId.ToString(),
            Title = payload.Title,
            Abstract = payload.Abstract,
            SpeakerId = payload.SpeakerId.ToString(),
            TalkTypeId = payload.TalkTypeId.ToString(),
            ConferenceId = payload.ConferenceId.ToString(),
            Tags = payload.Tags,
            Status = "Submitted",
            SubmittedAt = storedEvent.OccurredAt,
            LastModifiedAt = storedEvent.OccurredAt,
        };

        await _readModelRepository.Save(readModel);
    }

    public async Task HandleTalkTitleEdited(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TalkTitleEditedPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await _readModelRepository.GetById(storedEvent.AggregateId);
        if (readModel is null)
            return;

        readModel.Title = payload.Title;
        readModel.LastModifiedAt = storedEvent.OccurredAt;

        await _readModelRepository.Update(readModel);
    }

    public async Task HandleTalkAbstractEdited(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TalkAbstractEditedPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await _readModelRepository.GetById(storedEvent.AggregateId);
        if (readModel is null)
            return;

        readModel.Abstract = payload.Abstract;
        readModel.LastModifiedAt = storedEvent.OccurredAt;

        await _readModelRepository.Update(readModel);
    }

    public async Task HandleTalkTagAdded(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TalkTagPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await _readModelRepository.GetById(storedEvent.AggregateId);
        if (readModel is null)
            return;

        if (!readModel.Tags.Contains(payload.Tag))
        {
            readModel.Tags.Add(payload.Tag);
            readModel.LastModifiedAt = storedEvent.OccurredAt;
            await _readModelRepository.Update(readModel);
        }
    }

    public async Task HandleTalkTagRemoved(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TalkTagPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await _readModelRepository.GetById(storedEvent.AggregateId);
        if (readModel is null)
            return;

        if (readModel.Tags.Remove(payload.Tag))
        {
            readModel.LastModifiedAt = storedEvent.OccurredAt;
            await _readModelRepository.Update(readModel);
        }
    }

    // Event payload DTOs
    private record TalkSubmittedPayload(
        string Title,
        string Abstract,
        Guid SpeakerId,
        List<string> Tags,
        Guid TalkTypeId,
        Guid ConferenceId
    );

    private record TalkTitleEditedPayload(string Title);

    private record TalkAbstractEditedPayload(string Abstract);

    private record TalkTagPayload(string Tag);
}
