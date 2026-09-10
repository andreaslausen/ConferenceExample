using System.Text.Json;
using ConferenceExample.EventStore;
using ConferenceExample.Talk.Domain.TalkManagement;
using ConferenceExample.Talk.Persistence.ReadModels;

namespace ConferenceExample.Talk.Persistence.EventHandlers;

/// <summary>
/// Updates Talk read models in response to the Talk BC's own events. Each handler applies only the
/// delta carried by its event.
/// </summary>
public class TalkEventHandler(
    ITalkDocumentRepository readModelRepository,
    ITalkSubmissionDocumentRepository submissionRepository
)
{
    public async Task HandleTalkCreated(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TalkCreatedPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var newReadModel = new TalkDocument
        {
            Id = storedEvent.AggregateId.ToString(),
            Title = payload.Title,
            Abstract = payload.Abstract,
            SpeakerId = payload.SpeakerId.ToString(),
            Tags = payload.Tags,
            SubmissionCount = 0,
            CreatedAt = storedEvent.OccurredAt,
            LastModifiedAt = storedEvent.OccurredAt,
            Version = storedEvent.Version,
        };

        await readModelRepository.Save(newReadModel);
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

    /// <summary>
    /// Drops the talk from the speaker's list. Submission documents are left in place: they carry
    /// what the conferences received, and the conferences still hold those submissions.
    /// </summary>
    public async Task HandleTalkDeleted(StoredEvent storedEvent)
    {
        await readModelRepository.Delete(storedEvent.AggregateId);
    }

    /// <summary>
    /// Records the submission as Pending. It becomes Submitted, Accepted, Rejected or Failed once
    /// the Conference BC reports back — see <see cref="TalkSubmissionEventHandler"/>.
    /// </summary>
    public async Task HandleTalkSubmittedToConference(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<TalkSubmittedToConferencePayload>(
            storedEvent.Payload
        );
        if (payload is null)
            return;

        await submissionRepository.Save(
            new TalkSubmissionDocument
            {
                Id = TalkSubmissionDocument.BuildId(storedEvent.AggregateId, payload.ConferenceId),
                TalkId = storedEvent.AggregateId.ToString(),
                ConferenceId = payload.ConferenceId.ToString(),
                TalkTypeId = payload.TalkTypeId.ToString(),
                SpeakerId = payload.SpeakerId.ToString(),
                Status = SubmissionStatus.Pending.ToString(),
                SubmittedAt = storedEvent.OccurredAt,
                Title = payload.Title,
                Abstract = payload.Abstract,
                Tags = payload.Tags,
            }
        );

        var readModel = await readModelRepository.GetById(storedEvent.AggregateId);
        if (readModel is null)
            return;

        readModel.SubmissionCount++;
        readModel.LastModifiedAt = storedEvent.OccurredAt;
        readModel.Version = storedEvent.Version;

        await readModelRepository.Update(readModel);
    }

    private record TalkCreatedPayload(
        string Title,
        string Abstract,
        Guid SpeakerId,
        List<string> Tags
    );

    private record TalkSubmittedToConferencePayload(
        Guid ConferenceId,
        Guid TalkTypeId,
        Guid SpeakerId,
        string Title,
        string Abstract,
        List<string> Tags
    );

    private record TitlePayload(string Title);

    private record AbstractPayload(string Abstract);

    private record TagPayload(string Tag);
}
