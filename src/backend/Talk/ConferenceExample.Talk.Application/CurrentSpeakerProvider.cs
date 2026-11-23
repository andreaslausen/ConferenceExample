using ConferenceExample.Talk.Domain;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.ValueObjects.Ids;

namespace ConferenceExample.Talk.Application;

public class CurrentSpeakerProvider(
    ISpeakerReplica speakerReplica,
    ICurrentUserService currentUserService
) : ICurrentSpeakerProvider
{
    public async Task<SpeakerId> GetCurrentSpeakerId()
    {
        var userId = new UserId(new GuidV7(currentUserService.GetCurrentUserId()));

        return await speakerReplica.FindSpeakerIdByUserId(userId)
            ?? throw new NotFoundException(
                "No speaker profile exists for this account. Create a speaker profile first."
            );
    }
}
