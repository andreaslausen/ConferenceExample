using ConferenceExample.Talk.Domain.Entities;
using ConferenceExample.Talk.Domain.ValueObjects.Ids;

namespace ConferenceExample.Talk.Domain.Repositories;

public interface ISpeakerRepository
{
    Task<Speaker> GetSpeaker(SpeakerId speakerId);
    Task Save(Speaker speaker);
}
