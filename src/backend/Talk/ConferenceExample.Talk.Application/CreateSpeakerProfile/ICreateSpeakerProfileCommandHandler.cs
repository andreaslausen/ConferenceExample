namespace ConferenceExample.Talk.Application.CreateSpeakerProfile;

public interface ICreateSpeakerProfileCommandHandler
{
    Task Handle(CreateSpeakerProfileCommand command);
}
