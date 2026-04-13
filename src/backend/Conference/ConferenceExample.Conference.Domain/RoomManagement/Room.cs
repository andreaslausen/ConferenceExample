using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;

namespace ConferenceExample.Conference.Domain.RoomManagement;

public class Room(RoomId id, Text name)
{
    public RoomId Id { get; } = id;
    public Text Name { get; set; } = name;
}
