using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;

namespace ConferenceExample.Conference.Domain.ConferenceManagement;

/// <summary>
/// Entity representing a TalkType within a Conference.
/// TalkTypes define the different types of talks (e.g., Workshop, Keynote, Regular Talk)
/// that can be submitted to a conference.
/// </summary>
public class TalkType
{
    public TalkTypeId Id { get; }
    public Text Name { get; }

    internal TalkType(TalkTypeId id, Text name)
    {
        Id = id;
        Name = name;
    }
}
