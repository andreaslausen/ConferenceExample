namespace ConferenceExample.Talk.Application.CreateTalk;

public record CreateTalkCommand(string Title, string Abstract, IReadOnlyList<string> Tags);
