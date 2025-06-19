using ConferenceExample.Session.Domain.Entities;
using ConferenceExample.Session.Domain.ValueObjects.Ids;

namespace ConferenceExample.Session.Domain.Repositories;

public interface ISpeakerRepository
{
    Task<Speaker> GetSpeaker(SpeakerId speakerId);
    Task Save(Speaker speaker);
}