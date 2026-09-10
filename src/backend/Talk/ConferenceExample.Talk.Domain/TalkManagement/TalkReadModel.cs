namespace ConferenceExample.Talk.Domain.TalkManagement;

public record TalkReadModel(
    Guid Id,
    string Title,
    string Abstract,
    Guid SpeakerId,
    IReadOnlyList<string> Tags,
    int SubmissionCount
);
