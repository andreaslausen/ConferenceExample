namespace ConferenceExample.Talk.Domain.TalkManagement;

public record TalkTitle
{
    public TalkTitle(string title)
    {
        if (title.Length > 100)
        {
            throw new ArgumentException("The title must have a maximum of 100 characters", nameof(title));
        }
        Title = title;
    }

    public string Title { get; }
}
