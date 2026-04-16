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
        // Arrange
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();

        var expectedConferenceEventTypes = new[]
        {
            "ConferenceCreatedEvent",
            "ConferenceRenamedEvent",
            "ConferenceStatusChangedEvent",
        };

        // Act
        ConferenceEventSubscriptions.Subscribe(eventBus, scopeFactory);

        // Assert
        foreach (var eventType in expectedConferenceEventTypes)
        {
            eventBus.Received(1).Subscribe(eventType, Arg.Any<Func<StoredEvent, Task>>());
        }
    }

    [Fact]
    public void Subscribe_RegistersAllTalkDomainEvents()
    {
        // Arrange
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();

        var expectedTalkEventTypes = new[]
        {
            "TalkSubmittedEvent",
            "TalkTitleEditedEvent",
            "TalkAbstractEditedEvent",
            "TalkTagAddedEvent",
            "TalkTagRemovedEvent",
        };

        // Act
        ConferenceEventSubscriptions.Subscribe(eventBus, scopeFactory);

        // Assert
        foreach (var eventType in expectedTalkEventTypes)
        {
            eventBus.Received(1).Subscribe(eventType, Arg.Any<Func<StoredEvent, Task>>());
        }
    }

    [Fact]
    public void Subscribe_RegistersAllConferenceSpecificTalkEvents()
    {
        // Arrange
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();

        var expectedEventTypes = new[]
        {
            "TalkAcceptedEvent",
            "TalkRejectedEvent",
            "TalkScheduledEvent",
            "TalkAssignedToRoomEvent",
        };

        // Act
        ConferenceEventSubscriptions.Subscribe(eventBus, scopeFactory);

        // Assert
        foreach (var eventType in expectedEventTypes)
        {
            eventBus.Received(1).Subscribe(eventType, Arg.Any<Func<StoredEvent, Task>>());
        }
    }

    [Fact]
    public void Subscribe_RegistersExactlyTwelveEventTypes()
    {
        // Arrange
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();

        // Act
        ConferenceEventSubscriptions.Subscribe(eventBus, scopeFactory);

        // Assert - should register exactly 12 event types (3 conference + 5 talk + 4 conference-specific)
        eventBus.Received(12).Subscribe(Arg.Any<string>(), Arg.Any<Func<StoredEvent, Task>>());
    }

    [Fact]
    public async Task Subscribe_ConferenceEventHandler_CreatesScopeAndCallsHandler()
    {
        // Arrange
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var handler = Substitute.For<ConferenceEventHandler>(
            Substitute.For<IConferenceReadModelRepository>()
        );

        scopeFactory.CreateScope().Returns(scope);
        scope.ServiceProvider.Returns(serviceProvider);
        serviceProvider.GetService(typeof(ConferenceEventHandler)).Returns(handler);

        Func<StoredEvent, Task>? capturedHandler = null;
        eventBus
            .When(x => x.Subscribe("ConferenceCreatedEvent", Arg.Any<Func<StoredEvent, Task>>()))
            .Do(callInfo =>
            {
                capturedHandler = callInfo.ArgAt<Func<StoredEvent, Task>>(1);
            });

        ConferenceEventSubscriptions.Subscribe(eventBus, scopeFactory);

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ConferenceCreatedEvent",
            "{}",
            DateTimeOffset.UtcNow,
            1
        );

        // Act
        Assert.NotNull(capturedHandler);
        await capturedHandler!(storedEvent);

        // Assert
        scopeFactory.Received(1).CreateScope();
        scope.Received(1).Dispose();
    }

    [Fact]
    public async Task Subscribe_TalkEventHandler_CreatesScopeAndCallsHandler()
    {
        // Arrange
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var handler = Substitute.For<TalkEventHandler>(
            Substitute.For<IConferenceTalkReadModelRepository>()
        );

        scopeFactory.CreateScope().Returns(scope);
        scope.ServiceProvider.Returns(serviceProvider);
        serviceProvider.GetService(typeof(TalkEventHandler)).Returns(handler);

        Func<StoredEvent, Task>? capturedHandler = null;
        eventBus
            .When(x => x.Subscribe("TalkSubmittedEvent", Arg.Any<Func<StoredEvent, Task>>()))
            .Do(callInfo =>
            {
                capturedHandler = callInfo.ArgAt<Func<StoredEvent, Task>>(1);
            });

        ConferenceEventSubscriptions.Subscribe(eventBus, scopeFactory);

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TalkSubmittedEvent",
            "{}",
            DateTimeOffset.UtcNow,
            1
        );

        // Act
        Assert.NotNull(capturedHandler);
        await capturedHandler!(storedEvent);

        // Assert
        scopeFactory.Received(1).CreateScope();
        scope.Received(1).Dispose();
    }

    [Fact]
    public async Task Subscribe_TalkAcceptedEventHandler_CreatesScopeAndCallsHandler()
    {
        // Arrange
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var handler = Substitute.For<TalkEventHandler>(
            Substitute.For<IConferenceTalkReadModelRepository>()
        );

        scopeFactory.CreateScope().Returns(scope);
        scope.ServiceProvider.Returns(serviceProvider);
        serviceProvider.GetService(typeof(TalkEventHandler)).Returns(handler);

        Func<StoredEvent, Task>? capturedHandler = null;
        eventBus
            .When(x => x.Subscribe("TalkAcceptedEvent", Arg.Any<Func<StoredEvent, Task>>()))
            .Do(callInfo =>
            {
                capturedHandler = callInfo.ArgAt<Func<StoredEvent, Task>>(1);
            });

        ConferenceEventSubscriptions.Subscribe(eventBus, scopeFactory);

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TalkAcceptedEvent",
            "{}",
            DateTimeOffset.UtcNow,
            1
        );

        // Act
        Assert.NotNull(capturedHandler);
        await capturedHandler!(storedEvent);

        // Assert
        scopeFactory.Received(1).CreateScope();
        scope.Received(1).Dispose();
    }

    [Fact]
    public async Task Subscribe_TalkRejectedEventHandler_CreatesScopeAndCallsHandler()
    {
        // Arrange
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var handler = Substitute.For<TalkEventHandler>(
            Substitute.For<IConferenceTalkReadModelRepository>()
        );

        scopeFactory.CreateScope().Returns(scope);
        scope.ServiceProvider.Returns(serviceProvider);
        serviceProvider.GetService(typeof(TalkEventHandler)).Returns(handler);

        Func<StoredEvent, Task>? capturedHandler = null;
        eventBus
            .When(x => x.Subscribe("TalkRejectedEvent", Arg.Any<Func<StoredEvent, Task>>()))
            .Do(callInfo =>
            {
                capturedHandler = callInfo.ArgAt<Func<StoredEvent, Task>>(1);
            });

        ConferenceEventSubscriptions.Subscribe(eventBus, scopeFactory);

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TalkRejectedEvent",
            "{}",
            DateTimeOffset.UtcNow,
            1
        );

        // Act
        Assert.NotNull(capturedHandler);
        await capturedHandler!(storedEvent);

        // Assert
        scopeFactory.Received(1).CreateScope();
        scope.Received(1).Dispose();
    }

    [Fact]
    public async Task Subscribe_TalkScheduledEventHandler_CreatesScopeAndCallsHandler()
    {
        // Arrange
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var handler = Substitute.For<TalkEventHandler>(
            Substitute.For<IConferenceTalkReadModelRepository>()
        );

        scopeFactory.CreateScope().Returns(scope);
        scope.ServiceProvider.Returns(serviceProvider);
        serviceProvider.GetService(typeof(TalkEventHandler)).Returns(handler);

        Func<StoredEvent, Task>? capturedHandler = null;
        eventBus
            .When(x => x.Subscribe("TalkScheduledEvent", Arg.Any<Func<StoredEvent, Task>>()))
            .Do(callInfo =>
            {
                capturedHandler = callInfo.ArgAt<Func<StoredEvent, Task>>(1);
            });

        ConferenceEventSubscriptions.Subscribe(eventBus, scopeFactory);

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TalkScheduledEvent",
            "{}",
            DateTimeOffset.UtcNow,
            1
        );

        // Act
        Assert.NotNull(capturedHandler);
        await capturedHandler!(storedEvent);

        // Assert
        scopeFactory.Received(1).CreateScope();
        scope.Received(1).Dispose();
    }

    [Fact]
    public async Task Subscribe_TalkAssignedToRoomEventHandler_CreatesScopeAndCallsHandler()
    {
        // Arrange
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var handler = Substitute.For<TalkEventHandler>(
            Substitute.For<IConferenceTalkReadModelRepository>()
        );

        scopeFactory.CreateScope().Returns(scope);
        scope.ServiceProvider.Returns(serviceProvider);
        serviceProvider.GetService(typeof(TalkEventHandler)).Returns(handler);

        Func<StoredEvent, Task>? capturedHandler = null;
        eventBus
            .When(x => x.Subscribe("TalkAssignedToRoomEvent", Arg.Any<Func<StoredEvent, Task>>()))
            .Do(callInfo =>
            {
                capturedHandler = callInfo.ArgAt<Func<StoredEvent, Task>>(1);
            });

        ConferenceEventSubscriptions.Subscribe(eventBus, scopeFactory);

        var storedEvent = new StoredEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TalkAssignedToRoomEvent",
            "{}",
            DateTimeOffset.UtcNow,
            1
        );

        // Act
        Assert.NotNull(capturedHandler);
        await capturedHandler!(storedEvent);

        // Assert
        scopeFactory.Received(1).CreateScope();
        scope.Received(1).Dispose();
    }

    [Fact]
    public void Subscribe_CanBeCalledMultipleTimes()
    {
        // Arrange
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();

        // Act - call multiple times
        ConferenceEventSubscriptions.Subscribe(eventBus, scopeFactory);
        ConferenceEventSubscriptions.Subscribe(eventBus, scopeFactory);

        // Assert - each call should register handlers
        eventBus.Received(24).Subscribe(Arg.Any<string>(), Arg.Any<Func<StoredEvent, Task>>());
    }
}
