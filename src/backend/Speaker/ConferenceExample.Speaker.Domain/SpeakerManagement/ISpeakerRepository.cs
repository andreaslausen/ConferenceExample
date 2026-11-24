namespace ConferenceExample.Speaker.Domain.SpeakerManagement;

public interface ISpeakerRepository
{
    Task<Speaker> GetById(SpeakerId speakerId);

    Task Save(Speaker speaker);
}
