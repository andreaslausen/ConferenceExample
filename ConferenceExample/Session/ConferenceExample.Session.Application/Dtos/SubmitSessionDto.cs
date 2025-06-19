namespace ConferenceExample.Session.Application.Dtos;

public class SubmitSessionDto
{
    public required string Title { get; init; }
    public required string Abstract { get; init; }
    public required Guid ConferenceId { get; init; }
    public required Guid SpeakerId { get; init; }
    public required List<string> Tags { get; init; }
    public required Guid SessionTypeId { get; init; }
}