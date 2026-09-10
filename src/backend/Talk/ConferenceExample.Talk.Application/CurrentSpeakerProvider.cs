using ConferenceExample.Talk.Domain.SharedKernel;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;

namespace ConferenceExample.Talk.Application;

public class CurrentSpeakerProvider(
    ISpeakerDirectory speakerDirectory,
    ICurrentUserService currentUserService
) : ICurrentSpeakerProvider
{
    public async Task<SpeakerId> GetCurrentSpeakerId()
    {
        var userId = new UserId(new GuidV7(currentUserService.GetCurrentUserId()));

        return await speakerDirectory.FindSpeakerIdByUserId(userId)
            ?? throw new NotFoundException(
                "No speaker profile exists for this account. Create a speaker profile first."
            );
    }
}
