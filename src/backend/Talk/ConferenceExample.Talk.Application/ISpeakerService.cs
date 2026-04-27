using ConferenceExample.Talk.Application.CreateSpeakerProfile;
using ConferenceExample.Talk.Application.GetMyProfile;
using ConferenceExample.Talk.Application.GetSpeakerById;
using ConferenceExample.Talk.Application.UpdateSpeakerProfile;

namespace ConferenceExample.Talk.Application;

public interface ISpeakerService
{
    Task<SpeakerProfileCreatedDto> CreateProfile(CreateSpeakerProfileDto dto);
    Task UpdateProfile(UpdateSpeakerProfileDto dto);
    Task<GetMyProfileDto?> GetMyProfile();
    Task<GetSpeakerByIdDto?> GetSpeakerById(Guid speakerId);
}
