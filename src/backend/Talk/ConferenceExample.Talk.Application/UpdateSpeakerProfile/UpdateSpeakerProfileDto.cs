namespace ConferenceExample.Talk.Application.UpdateSpeakerProfile;

public class UpdateSpeakerProfileDto
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Biography { get; init; }
}
