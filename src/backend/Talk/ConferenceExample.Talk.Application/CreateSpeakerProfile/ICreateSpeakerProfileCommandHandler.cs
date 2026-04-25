namespace ConferenceExample.Talk.Application.CreateSpeakerProfile;

public interface ICreateSpeakerProfileCommandHandler
{
    Task<SpeakerProfileCreatedDto> Handle(CreateSpeakerProfileCommand command);
}
