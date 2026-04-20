namespace ConferenceExample.Talk.Domain.SpeakerManagement;

public interface ISpeakerReadModelRepository
{
    Task<SpeakerReadModel?> GetById(SpeakerId speakerId);
}
