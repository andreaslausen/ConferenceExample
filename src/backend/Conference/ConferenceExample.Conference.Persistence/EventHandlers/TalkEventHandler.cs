using System.Text.Json;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Domain.TalkManagement;
using ConferenceExample.Conference.Domain.TalkManagement.Events;
using ConferenceExample.Conference.Persistence.ReadModels;
using ConferenceExample.EventStore;
using ConferenceAggregate = ConferenceExample.Conference.Domain.ConferenceManagement.Conference;

namespace ConferenceExample.Conference.Persistence.EventHandlers;

/// <summary>
/// Receives submissions from the Talk BC and keeps the Conference BC's denormalized
/// ConferenceTalkDocument read model in step with the conference's own decisions about them
/// (accept, reject, schedule, assign a room).
///
/// What a talk says is snapshotted once, when the submission is registered. The Talk BC's later
/// edit events are deliberately not subscribed to: a conference reviews and schedules what was
/// submitted, not whatever the talk has since become.
/// </summary>
public class TalkEventHandler(
    IConferenceTalkDocumentRepository readModelRepository,
    ISpeakerDocumentRepository speakerDirectory,
    IConferenceRepository conferenceRepository,
    IEventBus eventBus
)
{
    public async Task HandleTalkSubmittedToConference(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TalkSubmittedToConferencePayload>(
            storedEvent.Payload
        );
        if (payload is null)
            return;

        // The Talk BC raised this on the talk's own stream, so the aggregate id is the talk.
        var talkId = storedEvent.AggregateId;
        var conferenceId = new ConferenceId(new GuidV7(payload.ConferenceId));

        ConferenceAggregate conference;
        try
        {
            conference = await conferenceRepository.GetById(conferenceId);
        }
        catch (NotFoundException)
        {
            // No Conference aggregate exists to raise a domain event from, so this isn't a fact
            // about Conference's own history — it's purely an integration notification back to
            // Talk, published directly rather than persisted to Conference's event store.
            PublishSubmissionRejected(
                payload.ConferenceId,
                talkId,
                $"Conference {payload.ConferenceId} does not exist."
            );
            return;
        }

        conference.SubmitTalk(
            new TalkId(new GuidV7(talkId)),
            new TalkTypeId(new GuidV7(payload.TalkTypeId))
        );

        var registered = conference
            .GetUncommittedEvents()
            .OfType<TalkSubmissionRegisteredEvent>()
            .Any();

        await conferenceRepository.Save(conference);

        // Conference turns a submission away with a TalkSubmissionRejectedEvent instead — the talk
        // never entered review, so it gets no read model here either.
        if (!registered)
            return;

        var speaker = await speakerDirectory.GetById(payload.SpeakerId);

        var newReadModel = new ConferenceTalkDocument
        {
            Id = ConferenceTalkDocument.BuildId(payload.ConferenceId, talkId),
            TalkId = talkId.ToString(),
            ConferenceId = payload.ConferenceId.ToString(),
            Title = payload.Title,
            Abstract = payload.Abstract,
            SpeakerId = payload.SpeakerId.ToString(),
            SpeakerFirstName = speaker?.FirstName ?? string.Empty,
            SpeakerLastName = speaker?.LastName ?? string.Empty,
            SpeakerBiography = speaker?.Biography ?? string.Empty,
            TalkTypeId = payload.TalkTypeId.ToString(),
            Tags = payload.Tags,
            Status = TalkStatus.Submitted.ToString(),
            SubmittedAt = storedEvent.OccurredAt,
            LastModifiedAt = storedEvent.OccurredAt,
            Version = 0,
        };

        await readModelRepository.Save(newReadModel);
    }

    public Task HandleTalkAccepted(StoredEvent storedEvent) =>
        UpdateTalk(storedEvent, (document, _) => document.Status = TalkStatus.Accepted.ToString());

    public Task HandleTalkRejected(StoredEvent storedEvent) =>
        UpdateTalk(storedEvent, (document, _) => document.Status = TalkStatus.Rejected.ToString());

    public async Task HandleTalkScheduled(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TalkScheduledPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        await UpdateTalk(
            storedEvent,
            (document, _) =>
            {
                document.SlotStart = payload.TalkStart;
                document.SlotEnd = payload.TalkEnd;
            }
        );
    }

    public async Task HandleTalkAssignedToRoom(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TalkAssignedToRoomPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        await UpdateTalk(
            storedEvent,
            (document, _) =>
            {
                document.RoomId = payload.RoomId.ToString();
                document.RoomName = payload.RoomName;
            }
        );
    }

    /// <summary>
    /// All conference-side talk events are raised on the conference's stream, so the aggregate id
    /// identifies the conference and the payload names the talk within it.
    /// </summary>
    private async Task UpdateTalk(
        StoredEvent storedEvent,
        Action<ConferenceTalkDocument, StoredEvent> apply
    )
    {
        var payload = JsonSerializer.Deserialize<TalkIdPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var readModel = await readModelRepository.Get(storedEvent.AggregateId, payload.TalkId);
        if (readModel is null)
            return;

        apply(readModel, storedEvent);
        readModel.LastModifiedAt = storedEvent.OccurredAt;
        readModel.Version = storedEvent.Version;

        await readModelRepository.Update(readModel);
    }

    // Constructs and publishes a TalkSubmissionRejectedEvent directly on the bus, bypassing the
    // event store entirely — there is no Conference aggregate instance to raise it from, so
    // nothing is (or should be) persisted. Talk's TalkSubmissionEventHandler reads only
    // payload.TalkId and payload.Reason plus the aggregate id, so the version here is never
    // observed downstream.
    private void PublishSubmissionRejected(Guid conferenceId, Guid talkId, string reason)
    {
        eventBus.Publish(
            new StoredEvent(
                GuidV7.NewGuid(),
                conferenceId,
                nameof(TalkSubmissionRejectedEvent),
                JsonSerializer.Serialize(new { TalkId = talkId, Reason = reason }),
                DateTimeOffset.UtcNow,
                -1
            )
        );
    }

    private record TalkSubmittedToConferencePayload(
        Guid ConferenceId,
        Guid TalkTypeId,
        Guid SpeakerId,
        string Title,
        string Abstract,
        List<string> Tags
    );

    private record TalkIdPayload(Guid TalkId);

    private record TalkScheduledPayload(
        Guid TalkId,
        DateTimeOffset TalkStart,
        DateTimeOffset TalkEnd
    );

    private record TalkAssignedToRoomPayload(Guid TalkId, Guid RoomId, string RoomName);
}
