using System.Text.Json;
using ConferenceExample.EventStore;
using ConferenceExample.Talk.Persistence.ReadModels;

namespace ConferenceExample.Talk.Persistence.EventHandlers;

/// <summary>
/// Updates Talk Read Models in response to slim Talk domain events.
/// Each handler applies only the delta carried by its event.
/// </summary>
public class TalkEventHandler
{
    private readonly ITalkDocumentRepository _readModelRepository;

    public TalkEventHandler(ITalkDocumentRepository readModelRepository)
    {
        _readModelRepository = readModelRepository;
    }

    public async Task HandleTalkSubmitted(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TalkSubmittedPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var newReadModel = new TalkDocument
        {
            Id = storedEvent.AggregateId.ToString(),
            Title = payload.Title,
            Abstract = payload.Abstract,
            SpeakerId = payload.SpeakerId.ToString(),
            SpeakerFirstName = payload.SpeakerFirstName,
            SpeakerLastName = payload.SpeakerLastName,
            SpeakerBiography = payload.SpeakerBiography,
            TalkTypeId = payload.TalkTypeId.ToString(),
            ConferenceId = payload.ConferenceId.ToString(),
            Tags = payload.Tags,
            Status = payload.Status,
            SubmittedAt = storedEvent.OccurredAt,
            LastModifiedAt = storedEvent.OccurredAt,
            Version = storedEvent.Version,
        };

        await _readModelRepository.Save(newReadModel);
    }

    public async Task HandleTalkTitleEdited(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TitlePayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await _readModelRepository.GetById(storedEvent.AggregateId);
        if (readModel is null)
            return;

        readModel.Title = payload.Title;
        readModel.LastModifiedAt = storedEvent.OccurredAt;
        readModel.Version = storedEvent.Version;

        await _readModelRepository.Update(readModel);
    }

    public async Task HandleTalkAbstractEdited(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<AbstractPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await _readModelRepository.GetById(storedEvent.AggregateId);
        if (readModel is null)
            return;

        readModel.Abstract = payload.Abstract;
        readModel.LastModifiedAt = storedEvent.OccurredAt;
        readModel.Version = storedEvent.Version;

        await _readModelRepository.Update(readModel);
    }

    public async Task HandleTalkTagAdded(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TagPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await _readModelRepository.GetById(storedEvent.AggregateId);
        if (readModel is null)
            return;

        if (!readModel.Tags.Contains(payload.Tag))
        {
            readModel.Tags.Add(payload.Tag);
        }

        readModel.LastModifiedAt = storedEvent.OccurredAt;
        readModel.Version = storedEvent.Version;

        await _readModelRepository.Update(readModel);
    }

    public async Task HandleTalkTagRemoved(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TagPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await _readModelRepository.GetById(storedEvent.AggregateId);
        if (readModel is null)
            return;

        readModel.Tags.RemoveAll(t => t == payload.Tag);
        readModel.LastModifiedAt = storedEvent.OccurredAt;
        readModel.Version = storedEvent.Version;

        await _readModelRepository.Update(readModel);
    }

    private record TalkSubmittedPayload(
        string Title,
        string Abstract,
        Guid SpeakerId,
        string SpeakerFirstName,
        string SpeakerLastName,
        string SpeakerBiography,
        List<string> Tags,
        Guid TalkTypeId,
        Guid ConferenceId,
        string Status
    );

    private record TitlePayload(string Title);

    private record AbstractPayload(string Abstract);

    private record TagPayload(string Tag);
}
