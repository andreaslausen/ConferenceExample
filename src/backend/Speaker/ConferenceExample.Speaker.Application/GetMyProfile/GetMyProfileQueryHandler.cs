using ConferenceExample.Speaker.Domain.SpeakerManagement;
using ConferenceExample.Speaker.Domain.ValueObjects.Ids;

namespace ConferenceExample.Speaker.Application.GetMyProfile;

public class GetMyProfileQueryHandler(
    ISpeakerReadModelRepository speakerReadModelRepository,
    ICurrentUserService currentUserService
) : IGetMyProfileQueryHandler
{
    public async Task<GetMyProfileDto?> Handle(GetMyProfileQuery query)
    {
        var userId = new UserId(new GuidV7(currentUserService.GetCurrentUserId()));

        var speaker = await speakerReadModelRepository.GetByUserId(userId);

        if (speaker is null)
            return null;

        return new GetMyProfileDto(
            speaker.Id,
            speaker.FirstName,
            speaker.LastName,
            speaker.Biography
        );
    }
}
