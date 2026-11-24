namespace ConferenceExample.Speaker.Application.CreateSpeakerProfile;

public interface ICreateSpeakerProfileCommandHandler
{
    Task<SpeakerProfileCreatedDto> Handle(CreateSpeakerProfileCommand command);
}
