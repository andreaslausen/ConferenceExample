using System.Collections.Concurrent;

namespace ConferenceExample.EventStore.UnitTests;

public class InMemoryEventBusTests
{
    [Fact]
    public async Task Publish_DispatchesToSubscribedHandler()
    {
        await using var bus = new InMemoryEventBus();
        var received = new TaskCompletionSource<StoredEvent>();
        bus.Subscribe(
            "TestEvent",
            e =>
            {
                received.TrySetResult(e);
                return Task.CompletedTask;
            }
        );
        await bus.StartAsync(CancellationToken.None);

        var storedEvent = NewStoredEvent("TestEvent", 0);
        bus.Publish(storedEvent);

        var observed = await received.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Equal(storedEvent, observed);
    }

    [Fact]
    public async Task Publish_PreservesOrder()
    {
        await using var bus = new InMemoryEventBus();
        var ordered = new List<long>();
        var allReceived = new TaskCompletionSource();
        bus.Subscribe(
            "Ordered",
            e =>
            {
                ordered.Add(e.Version);
                if (ordered.Count == 100)
                {
                    allReceived.TrySetResult();
                }
                return Task.CompletedTask;
            }
        );
        await bus.StartAsync(CancellationToken.None);

        for (var i = 0; i < 100; i++)
        {
            bus.Publish(NewStoredEvent("Ordered", i));
        }

        await allReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(Enumerable.Range(0, 100).Select(i => (long)i), ordered);
    }

    [Fact]
    public async Task Publish_FromMultipleProducers_DeliversAllEvents()
    {
        await using var bus = new InMemoryEventBus();
        var counter = 0;
        var allReceived = new TaskCompletionSource();
        bus.Subscribe(
            "Concurrent",
            _ =>
            {
                if (Interlocked.Increment(ref counter) == 1000)
                {
                    allReceived.TrySetResult();
                }
                return Task.CompletedTask;
            }
        );
        await bus.StartAsync(CancellationToken.None);

        var producers = Enumerable
            .Range(0, 10)
            .Select(_ =>
                Task.Run(() =>
                {
                    for (var i = 0; i < 100; i++)
                    {
                        bus.Publish(NewStoredEvent("Concurrent", i));
                    }
                })
            )
            .ToArray();

        await Task.WhenAll(producers);
        await allReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(1000, counter);
    }

    [Fact]
    public async Task Publish_HandlerThrowing_DoesNotStopBus()
    {
        await using var bus = new InMemoryEventBus();
        var received = new TaskCompletionSource<StoredEvent>();
        bus.Subscribe("Throwing", _ => throw new InvalidOperationException("boom"));
        bus.Subscribe(
            "Throwing",
            e =>
            {
                received.TrySetResult(e);
                return Task.CompletedTask;
            }
        );
        await bus.StartAsync(CancellationToken.None);

        bus.Publish(NewStoredEvent("Throwing", 0));

        var observed = await received.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Equal(0, observed.Version);
    }

    [Fact]
    public async Task Publish_DuplicateId_IsDispatchedOnce()
    {
        await using var bus = new InMemoryEventBus();
        var count = 0;
        bus.Subscribe(
            "Once",
            _ =>
            {
                Interlocked.Increment(ref count);
                return Task.CompletedTask;
            }
        );
        await bus.StartAsync(CancellationToken.None);

        var id = Guid.CreateVersion7();
        var storedEvent = new StoredEvent(
            id,
            Guid.CreateVersion7(),
            "Once",
            "{}",
            DateTimeOffset.UtcNow,
            0
        );

        bus.Publish(storedEvent);
        bus.Publish(storedEvent);

        await Task.Delay(100);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Publish_NoSubscribers_IsNoOp()
    {
        await using var bus = new InMemoryEventBus();
        await bus.StartAsync(CancellationToken.None);

        bus.Publish(NewStoredEvent("NoSubscriber", 0));

        // Give the background worker a moment to consume the event.
        await Task.Delay(50);
        // No exception, no observable side-effects.
        Assert.True(true);
    }

    [Fact]
    public async Task Subscribe_FromMultipleThreads_IsSafe()
    {
        await using var bus = new InMemoryEventBus();
        var tasks = Enumerable
            .Range(0, 10)
            .Select(i =>
                Task.Run(() =>
                {
                    bus.Subscribe($"Type{i % 3}", _ => Task.CompletedTask);
                })
            )
            .ToArray();

        await Task.WhenAll(tasks);
        Assert.True(true);
    }

    [Fact]
    public async Task StopAsync_StopsAcceptingEvents()
    {
        await using var bus = new InMemoryEventBus();
        await bus.StartAsync(CancellationToken.None);
        await bus.StopAsync(CancellationToken.None);

        Assert.Throws<InvalidOperationException>(() =>
            bus.Publish(NewStoredEvent("Anything", 0))
        );
    }

    [Fact]
    public async Task Handlers_ForSameEvent_RunSequentially()
    {
        await using var bus = new InMemoryEventBus();
        var executions = new ConcurrentQueue<int>();
        bus.Subscribe(
            "Seq",
            async _ =>
            {
                executions.Enqueue(1);
                await Task.Delay(20);
                executions.Enqueue(2);
            }
        );
        bus.Subscribe(
            "Seq",
            _ =>
            {
                executions.Enqueue(3);
                return Task.CompletedTask;
            }
        );
        await bus.StartAsync(CancellationToken.None);

        bus.Publish(NewStoredEvent("Seq", 0));

        await Task.Delay(200);
        var snapshot = executions.ToArray();
        Assert.Equal([1, 2, 3], snapshot);
    }

    private static StoredEvent NewStoredEvent(string eventType, long version) =>
        new(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            eventType,
            "{}",
            DateTimeOffset.UtcNow,
            version
        );
}
