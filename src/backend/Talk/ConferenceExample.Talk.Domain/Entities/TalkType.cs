using ConferenceExample.Talk.Domain.ValueObjects;
using ConferenceExample.Talk.Domain.ValueObjects.Ids;

namespace ConferenceExample.Talk.Domain.Entities;

public class TalkType(TalkTypeId id, Text name)
{
    public TalkTypeId Id { get; } = id;
    public Text Name { get; } = name;
}
