using ConferenceExample.EventStore;
using ConferenceExample.Talk.Application;
using ConferenceExample.Talk.Application.SubmitTalk;
using ConferenceExample.Talk.Domain.TalkManagement;
using ConferenceExample.Talk.Persistence;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Reqnroll.Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Talk.AcceptanceTests;

public class SetupTestDependencies
{
    [ScenarioDependencies]
    public static IServiceCollection CreateServices()
    {
        var services = new ServiceCollection();

        // Use in-memory mock implementations for testing
        var eventStore = Substitute.For<IEventStore>();

        // Configure eventStore to use in-memory storage
        var storage = new Dictionary<Guid, List<StoredEvent>>();
        eventStore
            .When(x =>
                x.AppendEvents(
                    Arg.Any<Guid>(),
                    Arg.Any<IEnumerable<StoredEvent>>(),
                    Arg.Any<long>()
                )
            )
            .Do(callInfo =>
            {
                var aggregateId = callInfo.ArgAt<Guid>(0);
                var events = callInfo.ArgAt<IEnumerable<StoredEvent>>(1);
                if (!storage.ContainsKey(aggregateId))
                    storage[aggregateId] = new List<StoredEvent>();
                storage[aggregateId].AddRange(events);
            });

        eventStore
            .GetEvents(Arg.Any<Guid>())
            .Returns(callInfo =>
            {
                var aggregateId = callInfo.ArgAt<Guid>(0);
                return storage.TryGetValue(aggregateId, out var events)
                    ? (IReadOnlyList<StoredEvent>)events.OrderBy(e => e.Version).ToList()
                    : new List<StoredEvent>();
            });

        eventStore
            .GetAllEvents()
            .Returns(callInfo =>
            {
                var allEvents = storage.Values.SelectMany(e => e).OrderBy(e => e.Version).ToList();
                return (IReadOnlyList<StoredEvent>)allEvents;
            });

        services.AddSingleton(eventStore);
        services.AddScoped<ITalkRepository, TalkRepository>();
        services.AddScoped<ISubmitTalkCommandHandler, SubmitTalkCommandHandler>();
        services.AddScoped<ITalkService, TalkService>();
        return services;
    }
}
