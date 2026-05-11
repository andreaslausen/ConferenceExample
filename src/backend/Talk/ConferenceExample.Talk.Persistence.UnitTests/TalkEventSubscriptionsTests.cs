using ConferenceExample.EventStore;
using ConferenceExample.Talk.Persistence.EventHandlers;
using ConferenceExample.Talk.Persistence.EventSubscriptions;
using ConferenceExample.Talk.Persistence.ReadModels;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace ConferenceExample.Talk.Persistence.UnitTests;

public class TalkEventSubscriptionsTests
{
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
        };

        TalkEventSubscriptions.Subscribe(eventBus, scopeFactory);

        foreach (var eventType in expectedEventTypes)
        {
            eventBus.Received(1).Subscribe(eventType, Arg.Any<Func<StoredEvent, Task>>());
        }
    }

    [Fact]
    public void Subscribe_RegistersAllSpeakerDomainEvents()
    {
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();

        var expectedEventTypes = new[]
        {
            "SpeakerProfileCreatedEvent",
            "SpeakerProfileUpdatedEvent",
        };

        TalkEventSubscriptions.Subscribe(eventBus, scopeFactory);

        foreach (var eventType in expectedEventTypes)
        {
            eventBus.Received(1).Subscribe(eventType, Arg.Any<Func<StoredEvent, Task>>());
        }
    }

    [Fact]
    public void Subscribe_RegistersAllConferenceReplicationEvents()
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
            "RoomAddedEvent",
            "RoomRemovedEvent",
            "TalkSubmittedToConferenceEvent",
            "TalkAcceptedEvent",
            "TalkRejectedEvent",
            "TalkScheduledEvent",
            "TalkAssignedToRoomEvent",
        };

        TalkEventSubscriptions.Subscribe(eventBus, scopeFactory);

        foreach (var eventType in expectedEventTypes)
        {
            eventBus.Received(1).Subscribe(eventType, Arg.Any<Func<StoredEvent, Task>>());
        }
    }

    [Fact]
    public void Subscribe_RegistersExactlyTwentyEventTypes()
    {
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();

        TalkEventSubscriptions.Subscribe(eventBus, scopeFactory);

        // 13 conference replication events + 5 talk events + 2 speaker events
        eventBus.Received(20).Subscribe(Arg.Any<string>(), Arg.Any<Func<StoredEvent, Task>>());
    }

    [Fact]
    public async Task Subscribe_TalkSubmittedHandler_ResolvesAndRunsHandler()
    {
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var handler = Substitute.For<TalkEventHandler>(
            Substitute.For<ITalkDocumentRepository>()
        );

        scopeFactory.CreateScope().Returns(scope);
        scope.ServiceProvider.Returns(serviceProvider);
        serviceProvider.GetService(typeof(TalkEventHandler)).Returns(handler);

        Func<StoredEvent, Task>? captured = null;
        eventBus
            .When(x => x.Subscribe("TalkSubmittedEvent", Arg.Any<Func<StoredEvent, Task>>()))
            .Do(callInfo => captured = callInfo.ArgAt<Func<StoredEvent, Task>>(1));

        TalkEventSubscriptions.Subscribe(eventBus, scopeFactory);

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "TalkSubmittedEvent",
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
    public void Subscribe_CanBeCalledMultipleTimes()
    {
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();

        TalkEventSubscriptions.Subscribe(eventBus, scopeFactory);
        TalkEventSubscriptions.Subscribe(eventBus, scopeFactory);

        eventBus.Received(40).Subscribe(Arg.Any<string>(), Arg.Any<Func<StoredEvent, Task>>());
    }
}
