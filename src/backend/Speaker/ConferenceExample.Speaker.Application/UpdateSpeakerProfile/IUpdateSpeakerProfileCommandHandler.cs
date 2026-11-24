namespace ConferenceExample.Speaker.Application.UpdateSpeakerProfile;

public interface IUpdateSpeakerProfileCommandHandler
{
    Task Handle(UpdateSpeakerProfileCommand command);
}
