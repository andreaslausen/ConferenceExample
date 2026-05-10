using ConferenceExample.Conference.Persistence.EventHandlers;
using ConferenceExample.Conference.Persistence.EventSubscriptions;
using ConferenceExample.Conference.Persistence.ReadModels;
using ConferenceExample.EventStore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

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
        var handler = Substitute.For<TalkEventHandler>(
            Substitute.For<IConferenceTalkDocumentRepository>()
        );

        scopeFactory.CreateScope().Returns(scope);
        scope.ServiceProvider.Returns(serviceProvider);
        serviceProvider.GetService(typeof(TalkEventHandler)).Returns(handler);

        Func<StoredEvent, Task>? captured = null;
        eventBus
            .When(x => x.Subscribe("TalkSubmittedEvent", Arg.Any<Func<StoredEvent, Task>>()))
            .Do(callInfo => captured = callInfo.ArgAt<Func<StoredEvent, Task>>(1));

        ConferenceEventSubscriptions.Subscribe(eventBus, scopeFactory);

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

        ConferenceEventSubscriptions.Subscribe(eventBus, scopeFactory);
        ConferenceEventSubscriptions.Subscribe(eventBus, scopeFactory);

        eventBus.Received(30).Subscribe(Arg.Any<string>(), Arg.Any<Func<StoredEvent, Task>>());
    }
}
