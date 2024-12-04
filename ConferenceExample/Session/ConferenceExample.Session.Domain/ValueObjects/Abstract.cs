namespace ConferenceExample.Session.Domain.ValueObjects;

public record Abstract
{
    public Abstract(string content)
    {
        if (content.Length > 1000)
        {
            throw new ArgumentException("The abstract must have a maximum of 1000 characters", nameof(content));
        }
        Content = content;
    }

    public string Content { get; }
}