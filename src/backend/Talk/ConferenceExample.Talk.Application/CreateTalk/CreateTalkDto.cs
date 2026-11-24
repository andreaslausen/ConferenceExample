namespace ConferenceExample.Talk.Application.CreateTalk;

public record CreateTalkDto(string Title, string Abstract, IReadOnlyList<string> Tags);
