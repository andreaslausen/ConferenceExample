using ConferenceExample.Session.Domain.ValueObjects;
using ConferenceExample.Session.Domain.ValueObjects.Ids;

namespace ConferenceExample.Session.Domain.Entities;

public class SessionType(SessionTypeId id, Text name)
{
    public SessionTypeId Id { get; } = id;
    public Text Name { get; } = name;
}