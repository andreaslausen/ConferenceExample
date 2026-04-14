using System.Text.Json;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Domain.TalkManagement;
using ConferenceExample.EventStore;

namespace ConferenceExample.Conference.Persistence;

/// <summary>
/// Repository for Talk Read Models in the Conference bounded context.
/// Currently reads from Event Store by reconstructing talk data from Talk BC events.
/// Later will read from dedicated Read Model projections.
/// </summary>
public class TalkRepository(IEventStore eventStore) : ITalkRepository
{
    // We only care about TalkSubmitted events from the Talk BC for now
    // Later, when we have read models, this will query a denormalized table
    private static readonly HashSet<string> TalkEventTypes =
    [
        "TalkSubmittedEvent",
        "TalkTitleEditedEvent",
        "TalkAbstractEditedEvent",
        "TalkTagAddedEvent",
        "TalkTagRemovedEvent",
    ];

    public async Task<IReadOnlyList<Talk>> GetTalksByConferenceId(
        Domain.ConferenceManagement.ConferenceId conferenceId
    )
    {
        var allEvents = await eventStore.GetAllEvents();

        // Group events by aggregate ID to reconstruct each talk
        var talkEvents = allEvents
            .Where(e => TalkEventTypes.Contains(e.EventType))
            .GroupBy(e => e.AggregateId);

        var talks = new List<Talk>();

        foreach (var group in talkEvents)
        {
            var orderedEvents = group.OrderBy(e => e.Version).ToList();

            // Get the TalkSubmittedEvent to check conference ID and get initial data
            var submittedEvent = orderedEvents.FirstOrDefault(e =>
                e.EventType == "TalkSubmittedEvent"
            );

            if (submittedEvent == null)
                continue;

            var submittedData = JsonSerializer.Deserialize<TalkSubmittedEventData>(
                submittedEvent.Payload
            );

            if (submittedData == null)
                continue;

            var eventConferenceId = new GuidV7(submittedData.ConferenceId);
            if (!eventConferenceId.Equals(conferenceId.Value))
                continue;

            // Build the talk read model by applying all events
            var talkId = new TalkId(new GuidV7(submittedEvent.AggregateId));
            var title = new Text(submittedData.Title);
            var @abstract = new Text(submittedData.Abstract);
            var speakerId = new GuidV7(submittedData.SpeakerId);
            var talkTypeId = new GuidV7(submittedData.TalkTypeId);
            var tags = new List<string>(submittedData.Tags);

            // Apply subsequent events (title edits, abstract edits, tag changes)
            foreach (var evt in orderedEvents.Skip(1))
            {
                switch (evt.EventType)
                {
                    case "TalkTitleEditedEvent":
                        var titleEditData = JsonSerializer.Deserialize<TalkTitleEditedEventData>(
                            evt.Payload
                        );
                        if (titleEditData != null)
                            title = new Text(titleEditData.Title);
                        break;

                    case "TalkAbstractEditedEvent":
                        var abstractEditData =
                            JsonSerializer.Deserialize<TalkAbstractEditedEventData>(evt.Payload);
                        if (abstractEditData != null)
                            @abstract = new Text(abstractEditData.Abstract);
                        break;

                    case "TalkTagAddedEvent":
                        var tagAddedData = JsonSerializer.Deserialize<TalkTagAddedEventData>(
                            evt.Payload
                        );
                        if (tagAddedData != null && !tags.Contains(tagAddedData.Tag))
                            tags.Add(tagAddedData.Tag);
                        break;

                    case "TalkTagRemovedEvent":
                        var tagRemovedData = JsonSerializer.Deserialize<TalkTagRemovedEventData>(
                            evt.Payload
                        );
                        if (tagRemovedData != null)
                            tags.Remove(tagRemovedData.Tag);
                        break;
                }
            }

            talks.Add(
                new Talk(
                    talkId,
                    title,
                    @abstract,
                    speakerId,
                    talkTypeId,
                    tags,
                    TalkStatus.Submitted // Default status from Talk BC perspective
                )
            );
        }

        return talks;
    }

    // DTOs for deserializing Talk BC events (we only need the fields we care about)
    private record TalkSubmittedEventData(
        string Title,
        string Abstract,
        Guid SpeakerId,
        List<string> Tags,
        Guid TalkTypeId,
        Guid ConferenceId
    );

    private record TalkTitleEditedEventData(string Title);

    private record TalkAbstractEditedEventData(string Abstract);

    private record TalkTagAddedEventData(string Tag);

    private record TalkTagRemovedEventData(string Tag);
}
