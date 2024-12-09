namespace ConferenceExample.Persistence.Model;

public class Session
{
    public required long Id { get; init; }
    public required string Title { get; init; }
    public required string Abstract { get; init; }
    public required long ConferenceId { get; init; }
    public required long SpeakerId { get; init; }
    public required List<string> Tags { get; init; }
    public required long SessionTypeId { get; init; }
    public required int SessionStatus { get; init; }
    public long? RoomId { get; init; }
    public DateTimeOffset? StartTime { get; init; }
    public DateTimeOffset? EndTime { get; init; }
}