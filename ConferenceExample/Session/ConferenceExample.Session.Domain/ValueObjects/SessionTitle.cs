namespace ConferenceExample.Session.Domain.ValueObjects;

public record SessionTitle
{
    public SessionTitle(string title)
    {
        if (title.Length > 100)
        {
            throw new ArgumentException("The title must have a maximum of 100 characters", nameof(title));
        }
        Title = title;
    }

    public string Title { get; }
}