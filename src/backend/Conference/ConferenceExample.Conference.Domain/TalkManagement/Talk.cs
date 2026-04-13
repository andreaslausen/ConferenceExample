using ConferenceExample.Conference.Domain.RoomManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;

namespace ConferenceExample.Conference.Domain.TalkManagement;

public class Talk
{
    public TalkId Id { get; }
    public TalkStatus Status { get; private set; }
    public Time? Slot { get; private set; }
    public Room? Room { get; private set; }

    internal Talk(TalkId id)
    {
        Id = id;
        Status = TalkStatus.Submitted;
    }

    internal void Accept() => Status = TalkStatus.Accepted;

    internal void Reject() => Status = TalkStatus.Rejected;

    internal void Schedule(Time slot) => Slot = slot;

    internal void AssignRoom(RoomId roomId, Text roomName) => Room = new Room(roomId, roomName);
}
