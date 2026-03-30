namespace ConferenceExample.Session.Domain.ValueObjects;

public record SpeakerBiography
{
    public SpeakerBiography(string content)
    {
        if (content.Length > 2000)
        {
            throw new ArgumentException("The biography must have a maximum of 2000 characters", nameof(content));
        }
        Content = content;
    }

    public string Content { get; }
}