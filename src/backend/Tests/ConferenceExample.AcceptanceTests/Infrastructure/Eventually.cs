namespace ConferenceExample.AcceptanceTests.Infrastructure;

/// <summary>
/// Read models are projected asynchronously from stored events (see InMemoryEventBus), and a
/// submission additionally makes a round trip to the Conference BC and back. Steps that assert on
/// projected state therefore poll instead of asserting once.
/// </summary>
public static class Eventually
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(50);

    public static async Task<T> Succeeds<T>(Func<Task<T?>> probe, string description)
        where T : class
    {
        var deadline = DateTimeOffset.UtcNow.Add(Timeout);

        do
        {
            var result = await probe();
            if (result is not null)
                return result;

            await Task.Delay(PollInterval);
        } while (DateTimeOffset.UtcNow < deadline);

        throw new TimeoutException($"{description} did not happen within {Timeout.TotalSeconds}s.");
    }
}
