using System.Text.Json;
using ConferenceExample.EventStore;
using ConferenceExample.Talk.Domain.TalkManagement;
using ConferenceExample.Talk.Persistence.ReadModels;

namespace ConferenceExample.Talk.Persistence.EventHandlers;

/// <summary>
/// Projects the outcome of a submission back to the speaker. All four events are raised by the
/// Conference BC, so <c>storedEvent.AggregateId</c> is the ConferenceId and the talk is identified
/// by the payload's TalkId. The Conference's stream version is deliberately not stamped onto the
/// document: it belongs to another aggregate's history.
/// </summary>
public class TalkSubmissionEventHandler(ITalkSubmissionDocumentRepository submissionRepository)
{
    public Task HandleSubmissionRegistered(StoredEvent storedEvent) =>
        ApplyStatus(storedEvent, SubmissionStatus.Submitted, reason: null);

    public Task HandleTalkAccepted(StoredEvent storedEvent) =>
        ApplyStatus(storedEvent, SubmissionStatus.Accepted, reason: null);

    public Task HandleTalkRejected(StoredEvent storedEvent) =>
        ApplyStatus(storedEvent, SubmissionStatus.Rejected, reason: null);

    public async Task HandleSubmissionRejected(StoredEvent storedEvent)
    {
        var payload = JsonSerializer.Deserialize<SubmissionRejectedPayload>(storedEvent.Payload);
        if (payload is null)
            return;

        await ApplyStatus(storedEvent, SubmissionStatus.Failed, payload.Reason);
    }

    private async Task ApplyStatus(StoredEvent storedEvent, SubmissionStatus status, string? reason)
    {
        var payload = JsonSerializer.Deserialize<TalkReferencePayload>(storedEvent.Payload);
        if (payload is null)
            return;

        var document = await submissionRepository.Get(payload.TalkId, storedEvent.AggregateId);
        if (document is null)
            return;

        document.Status = status.ToString();
        document.Reason = reason;

        await submissionRepository.Update(document);
    }

    private record TalkReferencePayload(Guid TalkId);

    private record SubmissionRejectedPayload(Guid TalkId, string Reason);
}
