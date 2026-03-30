namespace ConferenceExample.Session.Domain.ValueObjects;

public record SessionTag
{
    public SessionTag(string tag)
    {
        if (tag.Length > 20)
        {
            throw new ArgumentException("The tag must have a maximum of 20 characters", nameof(tag));
        }
        Tag = tag;
    }

    public string Tag { get; }
}