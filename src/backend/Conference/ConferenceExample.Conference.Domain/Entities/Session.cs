using ConferenceExample.Conference.Domain.ValueObjects;
using ConferenceExample.Conference.Domain.ValueObjects.Ids;

namespace ConferenceExample.Conference.Domain.Entities;

public class Session(
    SessionId id,
    ConferenceId conferenceId,
    SessionStatus status,
    Time? slot,
    Room? room)
{
    public SessionId Id { get; } = id;
    public ConferenceId ConferenceId { get; } = conferenceId;
    public SessionStatus Status { get; private set; } = status;
    public Time? Slot { get; private set; } = slot;
    public Room? Room { get; private set; } = room;

    public void Submit()
    {
        Status = SessionStatus.Submitted;
    }

    public void Accept()
    {
        Status = SessionStatus.Accepted;
    }

    public void Reject()
    {
        Status = SessionStatus.Rejected;
    }

    public void Schedule(Time slot)
    {
        Slot = slot;
    }
    
    public void Assign(Room room)
    {
        Room = room;
    }
}