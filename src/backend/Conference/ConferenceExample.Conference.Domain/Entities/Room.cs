using ConferenceExample.Conference.Domain.ValueObjects;
using ConferenceExample.Conference.Domain.ValueObjects.Ids;

namespace ConferenceExample.Conference.Domain.Entities;

public class Room(RoomId id, Text name)
{
    public RoomId Id { get; } = id;
    public Text Name { get; set; } = name;
}