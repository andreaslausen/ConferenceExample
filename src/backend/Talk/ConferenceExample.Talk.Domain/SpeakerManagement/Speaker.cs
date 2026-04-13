using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects;

namespace ConferenceExample.Talk.Domain.SpeakerManagement;

public class Speaker(SpeakerId id, Name name, SpeakerBiography biography)
{
    public SpeakerId Id { get; } = id;
    public Name Name { get; } = name;
    public SpeakerBiography Biography { get; } = biography;
}
