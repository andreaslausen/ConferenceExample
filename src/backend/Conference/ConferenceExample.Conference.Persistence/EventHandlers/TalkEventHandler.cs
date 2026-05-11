using System.Text.Json;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Domain.TalkManagement;
using ConferenceExample.Conference.Persistence.ReadModels;
using ConferenceExample.EventStore;

namespace ConferenceExample.Conference.Persistence.EventHandlers;

/// <summary>
/// Synchronizes Talk events from the Talk BC into the Conference BC's denormalized
/// ConferenceTalkDocument read model. Also handles Conference-side talk events
/// (accept/reject/schedule/assign) which carry only the TalkId.
/// </summary>
public class TalkEventHandler(
    IConferenceTalkDocumentRepository readModelRepository,
    IConferenceRepository conferenceRepository
)
{
    public async Task HandleTalkSubmitted(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TalkSubmittedPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var newReadModel = new ConferenceTalkDocument
        {
            Id = storedEvent.AggregateId.ToString(),
            ConferenceId = payload.ConferenceId.ToString(),
            Title = payload.Title,
            Abstract = payload.Abstract,
            SpeakerId = payload.SpeakerId.ToString(),
            SpeakerFirstName = payload.SpeakerFirstName,
            SpeakerLastName = payload.SpeakerLastName,
            SpeakerBiography = payload.SpeakerBiography,
            TalkTypeId = payload.TalkTypeId.ToString(),
            Tags = payload.Tags,
            Status = payload.Status,
            SubmittedAt = storedEvent.OccurredAt,
            LastModifiedAt = storedEvent.OccurredAt,
            Version = storedEvent.Version,
        };

        await readModelRepository.Save(newReadModel);

        var conference = await conferenceRepository.GetById(
            new ConferenceId(new GuidV7(payload.ConferenceId))
        );
        conference.SubmitTalk(new TalkId(new GuidV7(storedEvent.AggregateId)));
        await conferenceRepository.Save(conference);
    }

    public async Task HandleTalkTitleEdited(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TitlePayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await readModelRepository.GetById(storedEvent.AggregateId);
        if (readModel is null)
            return;

        readModel.Title = payload.Title;
        readModel.LastModifiedAt = storedEvent.OccurredAt;
        readModel.Version = storedEvent.Version;

        await readModelRepository.Update(readModel);
    }

    public async Task HandleTalkAbstractEdited(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<AbstractPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await readModelRepository.GetById(storedEvent.AggregateId);
        if (readModel is null)
            return;

        readModel.Abstract = payload.Abstract;
        readModel.LastModifiedAt = storedEvent.OccurredAt;
        readModel.Version = storedEvent.Version;

        await readModelRepository.Update(readModel);
    }

    public async Task HandleTalkTagAdded(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TagPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await readModelRepository.GetById(storedEvent.AggregateId);
        if (readModel is null)
            return;

        if (!readModel.Tags.Contains(payload.Tag))
        {
            readModel.Tags.Add(payload.Tag);
        }

        readModel.LastModifiedAt = storedEvent.OccurredAt;
        readModel.Version = storedEvent.Version;

        await readModelRepository.Update(readModel);
    }

    public async Task HandleTalkTagRemoved(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TagPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await readModelRepository.GetById(storedEvent.AggregateId);
        if (readModel is null)
            return;

        readModel.Tags.RemoveAll(t => t == payload.Tag);
        readModel.LastModifiedAt = storedEvent.OccurredAt;
        readModel.Version = storedEvent.Version;

        await readModelRepository.Update(readModel);
    }

    public async Task HandleTalkAccepted(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TalkIdPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await readModelRepository.GetById(payload.TalkId);
        if (readModel is null)
            return;

        readModel.Status = "Accepted";
        readModel.LastModifiedAt = storedEvent.OccurredAt;
        readModel.Version = storedEvent.Version;

        await readModelRepository.Update(readModel);
    }

    public async Task HandleTalkRejected(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TalkIdPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await readModelRepository.GetById(payload.TalkId);
        if (readModel is null)
            return;

        readModel.Status = "Rejected";
        readModel.LastModifiedAt = storedEvent.OccurredAt;
        readModel.Version = storedEvent.Version;

        await readModelRepository.Update(readModel);
    }

    public async Task HandleTalkScheduled(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TalkScheduledPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await readModelRepository.GetById(payload.TalkId);
        if (readModel is null)
            return;

        readModel.SlotStart = payload.TalkStart;
        readModel.SlotEnd = payload.TalkEnd;
        readModel.LastModifiedAt = storedEvent.OccurredAt;
        readModel.Version = storedEvent.Version;

        await readModelRepository.Update(readModel);
    }

    public async Task HandleTalkAssignedToRoom(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TalkAssignedToRoomPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await readModelRepository.GetById(payload.TalkId);
        if (readModel is null)
            return;

        readModel.RoomId = payload.RoomId.ToString();
        readModel.RoomName = payload.RoomName;
        readModel.LastModifiedAt = storedEvent.OccurredAt;
        readModel.Version = storedEvent.Version;

        await readModelRepository.Update(readModel);
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

    private record TalkIdPayload(Guid TalkId);

    private record TalkScheduledPayload(
        Guid TalkId,
        DateTimeOffset TalkStart,
        DateTimeOffset TalkEnd
    );

    private record TalkAssignedToRoomPayload(Guid TalkId, Guid RoomId, string RoomName);
}
