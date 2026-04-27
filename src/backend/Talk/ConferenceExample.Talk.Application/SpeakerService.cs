using ConferenceExample.Talk.Application.CreateSpeakerProfile;
using ConferenceExample.Talk.Application.GetMyProfile;
using ConferenceExample.Talk.Application.GetSpeakerById;
using ConferenceExample.Talk.Application.UpdateSpeakerProfile;

namespace ConferenceExample.Talk.Application;

public class SpeakerService(
    ICreateSpeakerProfileCommandHandler createSpeakerProfileCommandHandler,
    IUpdateSpeakerProfileCommandHandler updateSpeakerProfileCommandHandler,
    IGetMyProfileQueryHandler getMyProfileQueryHandler,
    IGetSpeakerByIdQueryHandler getSpeakerByIdQueryHandler
) : ISpeakerService
{
    public async Task<SpeakerProfileCreatedDto> CreateProfile(CreateSpeakerProfileDto dto)
    {
        var command = new CreateSpeakerProfileCommand(dto.FirstName, dto.LastName, dto.Biography);
        return await createSpeakerProfileCommandHandler.Handle(command);
    }

    public async Task UpdateProfile(UpdateSpeakerProfileDto dto)
    {
        var command = new UpdateSpeakerProfileCommand(dto.FirstName, dto.LastName, dto.Biography);
        await updateSpeakerProfileCommandHandler.Handle(command);
    }

    public async Task<GetMyProfileDto?> GetMyProfile()
    {
        return await getMyProfileQueryHandler.Handle(new GetMyProfileQuery());
    }

    public async Task<GetSpeakerByIdDto?> GetSpeakerById(Guid speakerId)
    {
        return await getSpeakerByIdQueryHandler.Handle(new GetSpeakerByIdQuery(speakerId));
    }
}
