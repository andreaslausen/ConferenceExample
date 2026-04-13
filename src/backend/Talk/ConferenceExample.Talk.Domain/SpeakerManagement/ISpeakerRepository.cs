namespace ConferenceExample.Talk.Domain.SpeakerManagement;

public interface ISpeakerRepository
{
    Task<Speaker> GetSpeaker(SpeakerId speakerId);
    Task Save(Speaker speaker);
}
