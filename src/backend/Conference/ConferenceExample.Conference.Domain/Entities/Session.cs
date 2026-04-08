using ConferenceExample.Conference.Domain.ValueObjects;
using ConferenceExample.Conference.Domain.ValueObjects.Ids;

namespace ConferenceExample.Conference.Domain.Entities;

public class Session
{
    public SessionId Id { get; }
    public SessionStatus Status { get; private set; }
    public Time? Slot { get; private set; }
    public Room? Room { get; private set; }

    internal Session(SessionId id)
    {
        Id = id;
        Status = SessionStatus.Submitted;
    }

    internal void Accept() => Status = SessionStatus.Accepted;

    internal void Reject() => Status = SessionStatus.Rejected;

    internal void Schedule(Time slot) => Slot = slot;

    internal void AssignRoom(RoomId roomId, Text roomName) => Room = new Room(roomId, roomName);
}
