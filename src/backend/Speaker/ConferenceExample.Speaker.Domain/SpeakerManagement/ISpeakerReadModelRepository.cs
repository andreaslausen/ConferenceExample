namespace ConferenceExample.Speaker.Domain.SpeakerManagement;

public interface ISpeakerReadModelRepository
{
    Task<SpeakerReadModel?> GetById(SpeakerId speakerId);

    Task<SpeakerReadModel?> GetByUserId(UserId userId);
}
