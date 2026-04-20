using ConferenceExample.Authentication;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;

namespace ConferenceExample.Talk.Application.CreateSpeakerProfile;

public class CreateSpeakerProfileCommandHandler(
    ISpeakerRepository speakerRepository,
    ICurrentUserService currentUserService
) : ICreateSpeakerProfileCommandHandler
{
    public async Task Handle(CreateSpeakerProfileCommand command)
    {
        var currentUserId = currentUserService.GetCurrentUserId();
        var speakerId = new SpeakerId(new GuidV7(currentUserId.Value.Value));

        var speaker = Speaker.Create(
            speakerId,
            new Name(command.FirstName, command.LastName),
            new SpeakerBiography(command.Biography)
        );

        await speakerRepository.Save(speaker);
    }
}
