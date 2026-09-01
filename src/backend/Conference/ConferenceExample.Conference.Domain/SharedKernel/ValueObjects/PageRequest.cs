namespace ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;

public record PageRequest
{
    public const int MaxPageSize = 100;

    public PageRequest(int page, int pageSize)
    {
        if (page < 1)
        {
            throw new ArgumentException("Page must be greater than or equal to 1.", nameof(page));
        }

        if (pageSize is < 1 or > MaxPageSize)
        {
            throw new ArgumentException(
                $"PageSize must be between 1 and {MaxPageSize}.",
                nameof(pageSize)
            );
        }

        Page = page;
        PageSize = pageSize;
    }

    public int Page { get; }
    public int PageSize { get; }
    public int Skip => (Page - 1) * PageSize;
}
