using ConferenceExample.Speaker.Domain;
using ConferenceExample.Speaker.Domain.SpeakerManagement;
using ConferenceExample.Speaker.Domain.ValueObjects;
using ConferenceExample.Speaker.Domain.ValueObjects.Ids;

namespace ConferenceExample.Speaker.Application.UpdateSpeakerProfile;

public class UpdateSpeakerProfileCommandHandler(
    ISpeakerRepository speakerRepository,
    ISpeakerLookup speakerLookup,
    ICurrentUserService currentUserService
) : IUpdateSpeakerProfileCommandHandler
{
    public async Task Handle(UpdateSpeakerProfileCommand command)
    {
        var userId = new UserId(new GuidV7(currentUserService.GetCurrentUserId()));

        var speakerId =
            await speakerLookup.FindSpeakerIdByUserId(userId)
            ?? throw new NotFoundException("No speaker profile exists for this account.");

        var speaker = await speakerRepository.GetById(speakerId);

        speaker.UpdateProfile(
            new Name(command.FirstName, command.LastName),
            new SpeakerBiography(command.Biography)
        );

        await speakerRepository.Save(speaker);
    }
}
