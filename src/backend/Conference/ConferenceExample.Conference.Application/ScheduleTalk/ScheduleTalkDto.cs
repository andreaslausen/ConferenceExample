namespace ConferenceExample.Conference.Application.ScheduleTalk;

public class ScheduleTalkDto
{
    public required DateTimeOffset Start { get; init; }
    public required DateTimeOffset End { get; init; }
}
