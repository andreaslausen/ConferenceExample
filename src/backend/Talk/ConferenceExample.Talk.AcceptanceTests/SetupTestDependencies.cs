using System.Text.Json;
using ConferenceExample.EventStore;
using ConferenceExample.Talk.Application;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.TalkManagement;
using ConferenceExample.Talk.Persistence;
using ConferenceExample.Talk.Persistence.ReadModels;
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
        var eventStore = Substitute.For<ITalkEventStore>();

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

        // Mock ICurrentUserService to return a fixed test user ID
        var speakerId = Guid.CreateVersion7();
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.GetCurrentUserId().Returns(speakerId);

        // Pre-seed speaker profile in event store
        var speakerProfileEvent = new StoredEvent(
            Guid.CreateVersion7(),
            speakerId,
            "SpeakerProfileCreatedEvent",
            JsonSerializer.Serialize(
                new
                {
                    AggregateId = speakerId,
                    OccurredAt = DateTimeOffset.UtcNow,
                    Version = 0L,
                    FirstName = "Jane",
                    LastName = "Doe",
                    Biography = "Test speaker biography.",
                }
            ),
            DateTimeOffset.UtcNow,
            1
        );
        storage[speakerId] = new List<StoredEvent> { speakerProfileEvent };

        services.AddSingleton<ITalkEventStore>(eventStore);
        services.AddSingleton(currentUserService);
        services.AddTalkPersistence();

        // Override MongoDB-dependent read model repos with no-op mocks (no IMongoDatabase in acceptance tests)
        services.AddScoped<ITalkDocumentRepository>(_ => Substitute.For<ITalkDocumentRepository>());
        services.AddScoped<ITalkReadModelRepository>(_ =>
            Substitute.For<ITalkReadModelRepository>()
        );

        services.AddTalkApplication();
        return services;
    }
}
