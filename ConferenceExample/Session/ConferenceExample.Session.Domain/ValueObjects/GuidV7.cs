using ConferenceExample.Session.Domain.Extensions;

namespace ConferenceExample.Session.Domain.ValueObjects;

public sealed record GuidV7
{
    public Guid Value { get; }

    public GuidV7(Guid value)
    {
        if (!value.IsGuidV7())
        {
            throw new ArgumentException("Value must be a valid UUIDv7.", nameof(value));
        }

        Value = value;
    }

    public static GuidV7 NewGuid() => new(Guid.CreateVersion7());

    public static GuidV7 Parse(string value)
    {
        if (!value.IsGuidV7())
        {
            throw new ArgumentException("Value must be a valid UUIDv7 string.", nameof(value));
        }

        return new GuidV7(Guid.Parse(value));
    }

    public static implicit operator Guid(GuidV7 guid) => guid.Value;
    public static implicit operator GuidV7(Guid guid) => new(guid);

    public override string ToString()
    {
        return Value.ToString();
    }
}
