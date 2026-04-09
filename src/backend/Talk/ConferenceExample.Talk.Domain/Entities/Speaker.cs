using ConferenceExample.Talk.Domain.ValueObjects;
using ConferenceExample.Talk.Domain.ValueObjects.Ids;

namespace ConferenceExample.Talk.Domain.Entities;

public class Speaker(SpeakerId id, Name name, SpeakerBiography biography)
{
    public SpeakerId Id { get; } = id;
    public Name Name { get; } = name;
    public SpeakerBiography Biography { get; } = biography;
}