namespace ConferenceExample.Talk.Application.UpdateSpeakerProfile;

public interface IUpdateSpeakerProfileCommandHandler
{
    Task Handle(UpdateSpeakerProfileCommand command);
}
