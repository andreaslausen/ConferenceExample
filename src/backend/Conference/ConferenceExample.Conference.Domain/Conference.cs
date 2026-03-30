using ConferenceExample.Conference.Domain.ValueObjects;
using ConferenceExample.Conference.Domain.ValueObjects.Ids;

namespace ConferenceExample.Conference.Domain;

public class Conference(ConferenceId id, Text name, Time conferenceTime, Location location)
{
    public ConferenceId Id { get; } = id;
    public Text Name { get; set; } = name;
    public Time ConferenceTime { get; } = conferenceTime;
    public Location Location { get; } = location;
}