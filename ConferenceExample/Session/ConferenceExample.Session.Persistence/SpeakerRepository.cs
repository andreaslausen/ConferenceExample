using ConferenceExample.Session.Domain.Entities;
using ConferenceExample.Session.Domain.Repositories;
using ConferenceExample.Session.Domain.ValueObjects.Ids;

namespace ConferenceExample.Session.Persistence;

public class SpeakerRepository : ISpeakerRepository
{
    public Task<Speaker> GetSpeaker(SpeakerId speakerId)
    {
        throw new NotImplementedException();
    }

    public Task Save(Speaker speaker)
    {
        throw new NotImplementedException();
    }
}