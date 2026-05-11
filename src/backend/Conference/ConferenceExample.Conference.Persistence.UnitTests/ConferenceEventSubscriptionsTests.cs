using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Persistence.EventHandlers;
using ConferenceExample.Conference.Persistence.EventSubscriptions;
using ConferenceExample.Conference.Persistence.ReadModels;
using ConferenceExample.EventStore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ConferenceAggregate = ConferenceExample.Conference.Domain.ConferenceManagement.Conference;

namespace ConferenceExample.Conference.Persistence.UnitTests;

public class ConferenceEventSubscriptionsTests
{
    [Fact]
    public void Subscribe_RegistersAllConferenceDomainEvents()
    {
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();

        var expectedEventTypes = new[]
        {
            "ConferenceCreatedEvent",
            "ConferenceRenamedEvent",
            "ConferenceDetailsUpdatedEvent",
            "ConferenceStatusChangedEvent",
            "TalkTypeDefinedEvent",
            "TalkTypeRemovedEvent",
        };

        ConferenceEventSubscriptions.Subscribe(eventBus, scopeFactory);

        foreach (var eventType in expectedEventTypes)
        {
            eventBus.Received(1).Subscribe(eventType, Arg.Any<Func<StoredEvent, Task>>());
        }
    }

    [Fact]
    public void Subscribe_RegistersAllTalkDomainEvents()
    {
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();

        var expectedEventTypes = new[]
        {
            "TalkSubmittedEvent",
            "TalkTitleEditedEvent",
            "TalkAbstractEditedEvent",
            "TalkTagAddedEvent",
            "TalkTagRemovedEvent",
            "TalkAcceptedEvent",
            "TalkRejectedEvent",
            "TalkScheduledEvent",
            "TalkAssignedToRoomEvent",
        };

        ConferenceEventSubscriptions.Subscribe(eventBus, scopeFactory);

        foreach (var eventType in expectedEventTypes)
        {
            eventBus.Received(1).Subscribe(eventType, Arg.Any<Func<StoredEvent, Task>>());
        }
    }

    [Fact]
    public void Subscribe_RegistersExactlyFifteenEventTypes()
    {
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();

        ConferenceEventSubscriptions.Subscribe(eventBus, scopeFactory);

        // 6 Conference events + 9 Talk events
        eventBus.Received(15).Subscribe(Arg.Any<string>(), Arg.Any<Func<StoredEvent, Task>>());
    }

    [Fact]
    public async Task Subscribe_ConferenceCreatedHandler_ResolvesAndRunsHandler()
    {
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var handler = Substitute.For<ConferenceEventHandler>(
            Substitute.For<IConferenceDocumentRepository>()
        );

        scopeFactory.CreateScope().Returns(scope);
        scope.ServiceProvider.Returns(serviceProvider);
        serviceProvider.GetService(typeof(ConferenceEventHandler)).Returns(handler);

        Func<StoredEvent, Task>? captured = null;
        eventBus
            .When(x => x.Subscribe("ConferenceCreatedEvent", Arg.Any<Func<StoredEvent, Task>>()))
            .Do(callInfo => captured = callInfo.ArgAt<Func<StoredEvent, Task>>(1));

        ConferenceEventSubscriptions.Subscribe(eventBus, scopeFactory);

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "ConferenceCreatedEvent",
            "{}",
            DateTimeOffset.UtcNow,
            0
        );

        Assert.NotNull(captured);
        await captured!(storedEvent);

        scopeFactory.Received(1).CreateScope();
        scope.Received(1).Dispose();
    }

    [Fact]
    public async Task Subscribe_TalkSubmittedHandler_ResolvesAndRunsHandler()
    {
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var conferenceRepo = Substitute.For<IConferenceRepository>();
        conferenceRepo.GetById(Arg.Any<ConferenceId>()).Returns(CreateConference());
        var handler = new TalkEventHandler(
            Substitute.For<IConferenceTalkDocumentRepository>(),
            conferenceRepo
        );

        scopeFactory.CreateScope().Returns(scope);
        scope.ServiceProvider.Returns(serviceProvider);
        serviceProvider.GetService(typeof(TalkEventHandler)).Returns(handler);

        Func<StoredEvent, Task>? captured = null;
        eventBus
            .When(x => x.Subscribe("TalkSubmittedEvent", Arg.Any<Func<StoredEvent, Task>>()))
            .Do(callInfo => captured = callInfo.ArgAt<Func<StoredEvent, Task>>(1));

        ConferenceEventSubscriptions.Subscribe(eventBus, scopeFactory);

        var payload = System.Text.Json.JsonSerializer.Serialize(
            new
            {
                Title = "Talk",
                Abstract = "Abstract",
                SpeakerId = Guid.CreateVersion7(),
                SpeakerFirstName = "Jane",
                SpeakerLastName = "Doe",
                SpeakerBiography = "Bio",
                Tags = Array.Empty<string>(),
                TalkTypeId = Guid.CreateVersion7(),
                ConferenceId = Guid.CreateVersion7(),
                Status = "Submitted",
            }
        );
        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "TalkSubmittedEvent",
            payload,
            DateTimeOffset.UtcNow,
            0
        );

        Assert.NotNull(captured);
        await captured!(storedEvent);

        scopeFactory.Received(1).CreateScope();
        scope.Received(1).Dispose();
    }

    [Fact]
    public void Subscribe_CanBeCalledMultipleTimes()
    {
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();

        ConferenceEventSubscriptions.Subscribe(eventBus, scopeFactory);
        ConferenceEventSubscriptions.Subscribe(eventBus, scopeFactory);

        eventBus.Received(30).Subscribe(Arg.Any<string>(), Arg.Any<Func<StoredEvent, Task>>());
    }

    private static ConferenceAggregate CreateConference() =>
        ConferenceAggregate.Create(
            new ConferenceId(GuidV7.NewGuid()),
            new Text("Test Conference"),
            new Time(DateTimeOffset.UtcNow.AddDays(30), DateTimeOffset.UtcNow.AddDays(32)),
            new Location(
                new Text("Test Venue"),
                new Address("123 Main St", "Springfield", "IL", "62701", "US")
            ),
            new OrganizerId(GuidV7.NewGuid())
        );
}
