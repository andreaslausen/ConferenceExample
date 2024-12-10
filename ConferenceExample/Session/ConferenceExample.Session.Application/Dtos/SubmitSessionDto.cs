namespace ConferenceExample.Session.Application.Dtos;

public class SubmitSessionDto
{
    public required string Title { get; init; }
    public required string Abstract { get; init; }
    public required long ConferenceId { get; init; }
    public required long SpeakerId { get; init; }
    public required List<string> Tags { get; init; }
    public required long SessionTypeId { get; init; }
}