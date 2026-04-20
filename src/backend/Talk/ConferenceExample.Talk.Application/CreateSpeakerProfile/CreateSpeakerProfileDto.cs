namespace ConferenceExample.Talk.Application.CreateSpeakerProfile;

public class CreateSpeakerProfileDto
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Biography { get; init; }
}
