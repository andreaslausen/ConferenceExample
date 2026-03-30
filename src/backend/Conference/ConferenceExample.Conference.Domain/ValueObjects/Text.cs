namespace ConferenceExample.Conference.Domain.ValueObjects;

public record Text
{
    public Text(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }
}