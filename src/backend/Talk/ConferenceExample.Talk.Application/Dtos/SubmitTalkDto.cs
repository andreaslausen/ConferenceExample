namespace ConferenceExample.Talk.Application.Dtos;

public class SubmitTalkDto
{
    public required string Title { get; init; }
    public required string Abstract { get; init; }
    public required Guid ConferenceId { get; init; }
    public required Guid SpeakerId { get; init; }
    public required List<string> Tags { get; init; }
    public required Guid TalkTypeId { get; init; }
}