using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects;

namespace ConferenceExample.Talk.Domain.TalkTypeManagement;

public class TalkType(TalkTypeId id, Text name)
{
    public TalkTypeId Id { get; } = id;
    public Text Name { get; } = name;
}
