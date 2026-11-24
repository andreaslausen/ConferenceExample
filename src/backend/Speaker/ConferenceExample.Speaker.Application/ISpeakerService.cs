using ConferenceExample.Speaker.Application.CreateSpeakerProfile;
using ConferenceExample.Speaker.Application.GetMyProfile;
using ConferenceExample.Speaker.Application.GetSpeakerById;
using ConferenceExample.Speaker.Application.UpdateSpeakerProfile;

namespace ConferenceExample.Speaker.Application;

public interface ISpeakerService
{
    Task<SpeakerProfileCreatedDto> CreateProfile(CreateSpeakerProfileDto dto);
    Task UpdateProfile(UpdateSpeakerProfileDto dto);
    Task<GetMyProfileDto?> GetMyProfile();
    Task<GetSpeakerByIdDto?> GetSpeakerById(Guid speakerId);
}
