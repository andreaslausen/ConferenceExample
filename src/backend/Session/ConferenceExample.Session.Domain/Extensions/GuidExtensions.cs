namespace ConferenceExample.Session.Domain.Extensions;

public static class GuidExtensions
{
    public static bool IsGuidV7(this Guid guid)
    {
        return guid.Version == 7;
    }
}