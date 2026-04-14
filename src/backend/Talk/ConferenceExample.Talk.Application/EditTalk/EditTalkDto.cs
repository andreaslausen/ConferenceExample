namespace ConferenceExample.Talk.Application.EditTalk;

public record EditTalkDto(string Title, string Abstract, IReadOnlyList<string> Tags);
