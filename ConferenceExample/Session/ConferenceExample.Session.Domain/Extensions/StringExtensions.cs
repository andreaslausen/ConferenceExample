namespace ConferenceExample.Session.Domain.Extensions;

public static class StringExtensions
{
    public static bool IsGuidV7(this string guidString)
    {
        if (string.IsNullOrWhiteSpace(guidString))
        {
            return false;
        }

        if (!Guid.TryParse(guidString, out var guid))
        {
            return false;
        }

        return guid.IsGuidV7();
    }
}