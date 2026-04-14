using ConferenceExample.Conference.Domain.RoomManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Conference.Domain.TalkManagement;

/// <summary>
/// Read Model: Represents a Talk from the perspective of the Conference bounded context.
/// This is NOT the Talk aggregate from the Talk bounded context.
/// Data will be synchronized via events from the Talk BC (later via Read Model projections).
/// </summary>
public class Talk
{
    // Original fields managed by Conference aggregate
    public TalkId Id { get; }
    public TalkStatus Status { get; private set; }
    public Time? Slot { get; private set; }
    public Room? Room { get; private set; }

    // Read Model fields - replicated from Talk BC
    public Text? Title { get; private set; }
    public Text? Abstract { get; private set; }
    public GuidV7? SpeakerId { get; private set; }
    public GuidV7? TalkTypeId { get; private set; }
    public IReadOnlyList<string> Tags { get; private set; } = new List<string>();

    internal Talk(TalkId id)
    {
        Id = id;
        Status = TalkStatus.Submitted;
    }

    // Constructor for read model (used by repository when loading from events)
    public Talk(
        TalkId id,
        Text? title,
        Text? @abstract,
        GuidV7? speakerId,
        GuidV7? talkTypeId,
        IReadOnlyList<string> tags,
        TalkStatus status,
        Time? slot = null,
        Room? room = null
    )
    {
        Id = id;
        Title = title;
        Abstract = @abstract;
        SpeakerId = speakerId;
        TalkTypeId = talkTypeId;
        Tags = tags;
        Status = status;
        Slot = slot;
        Room = room;
    }

    internal void Accept() => Status = TalkStatus.Accepted;

    internal void Reject() => Status = TalkStatus.Rejected;

    internal void Schedule(Time slot) => Slot = slot;

    internal void AssignRoom(RoomId roomId, Text roomName) => Room = new Room(roomId, roomName);
}
