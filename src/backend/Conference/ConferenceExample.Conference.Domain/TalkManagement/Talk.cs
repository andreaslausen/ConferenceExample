using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.RoomManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;

namespace ConferenceExample.Conference.Domain.TalkManagement;

/// <summary>
/// A talk submitted to this conference, as the conference tracks it: which talk, in which format,
/// where it stands in review, and when and where it is scheduled.
///
/// This is not the Talk aggregate from the Talk bounded context. What the talk actually says —
/// title, abstract, tags, and who is speaking — is a snapshot taken when the submission was
/// registered and lives in the ConferenceTalk read model; the conference deliberately does not
/// follow later edits the speaker makes to their talk.
/// </summary>
public class Talk
{
    public TalkId Id { get; }
    public TalkTypeId TalkTypeId { get; }
    public TalkStatus Status { get; private set; }
    public Time? Slot { get; private set; }
    public Room? Room { get; private set; }

    internal Talk(TalkId id, TalkTypeId talkTypeId)
    {
        Id = id;
        TalkTypeId = talkTypeId;
        Status = TalkStatus.Submitted;
    }

    internal void Accept() => Status = TalkStatus.Accepted;

    internal void Reject() => Status = TalkStatus.Rejected;

    internal void Schedule(Time slot) => Slot = slot;

    internal void AssignRoom(RoomId roomId, Text roomName) => Room = new Room(roomId, roomName);
}
