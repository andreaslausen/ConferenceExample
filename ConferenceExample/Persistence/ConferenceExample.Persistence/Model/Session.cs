namespace ConferenceExample.Persistence.Model;

public class Session
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Abstract { get; init; }
    public required Guid ConferenceId { get; init; }
    public required Guid SpeakerId { get; init; }
    public required List<string> Tags { get; init; }
    public required Guid SessionTypeId { get; init; }
    public required int SessionStatus { get; init; }
    public Guid? RoomId { get; init; }
    public DateTimeOffset? StartTime { get; init; }
    public DateTimeOffset? EndTime { get; init; }
}