using ConferenceExample.Session.Domain.ValueObjects;
using ConferenceExample.Session.Domain.ValueObjects.Ids;

namespace ConferenceExample.Session.Domain.Entities;

public class Speaker(SpeakerId id, Name name, SpeakerBiography biography)
{
    public SpeakerId Id { get; } = id;
    public Name Name { get; } = name;
    public SpeakerBiography Biography { get; set; } = biography;
}