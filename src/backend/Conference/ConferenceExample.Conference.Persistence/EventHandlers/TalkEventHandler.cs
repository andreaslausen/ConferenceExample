using System.Text.Json;
using ConferenceExample.Conference.Persistence.ReadModels;
using ConferenceExample.EventStore;

namespace ConferenceExample.Conference.Persistence.EventHandlers;

/// <summary>
/// Event handler that synchronizes Talk events from the Talk BC to the Conference BC.
/// Updates the Conference Talk Read Models when Talk domain events occur in the Talk BC.
/// This enables the Conference BC to have denormalized talk data without tight coupling.
/// </summary>
public class TalkEventHandler
{
    private readonly IConferenceTalkReadModelRepository _readModelRepository;

    public TalkEventHandler(IConferenceTalkReadModelRepository readModelRepository)
    {
        _readModelRepository = readModelRepository;
    }

    public async Task HandleTalkSubmitted(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TalkSubmittedPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = new ConferenceTalkReadModel
        {
            Id = storedEvent.AggregateId.ToString(),
            ConferenceId = payload.ConferenceId.ToString(),
            Title = payload.Title,
            Abstract = payload.Abstract,
            SpeakerId = payload.SpeakerId.ToString(),
            TalkTypeId = payload.TalkTypeId.ToString(),
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

    public async Task HandleTalkAccepted(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TalkStatusPayload>(storedEvent.Payload);
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
        var payload = JsonSerializer.Deserialize<TalkStatusPayload>(storedEvent.Payload);
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
        var payload = JsonSerializer.Deserialize<TalkScheduledPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await _readModelRepository.GetById(payload.TalkId);
        if (readModel is null)
            return;

        readModel.SlotStart = payload.Start;
        readModel.SlotEnd = payload.End;
        readModel.LastModifiedAt = storedEvent.OccurredAt;

        await _readModelRepository.Update(readModel);
    }

    public async Task HandleTalkAssignedToRoom(StoredEvent storedEvent)
    {
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

    // Event payload DTOs (from Talk BC)
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

    // Event payload DTOs (from Conference BC)
    private record TalkStatusPayload(Guid TalkId);

    private record TalkScheduledPayload(Guid TalkId, DateTimeOffset Start, DateTimeOffset End);

    private record TalkAssignedToRoomPayload(Guid TalkId, Guid RoomId, string RoomName);
}
