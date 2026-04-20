using ConferenceExample.Authentication;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;

namespace ConferenceExample.Talk.Application.GetMyProfile;

public class GetMyProfileQueryHandler(
    ISpeakerReadModelRepository speakerReadModelRepository,
    ICurrentUserService currentUserService
) : IGetMyProfileQueryHandler
{
    public async Task<GetMyProfileDto?> Handle(GetMyProfileQuery query)
    {
        var currentUserId = currentUserService.GetCurrentUserId();
        var speakerId = new SpeakerId(new GuidV7(currentUserId.Value.Value));

        var profile = await speakerReadModelRepository.GetById(speakerId);

        if (profile is null)
            return null;

        return new GetMyProfileDto(
            profile.Id,
            profile.FirstName,
            profile.LastName,
            profile.Biography
        );
    }
}
