using ConferenceExample.EventStore;
using ConferenceExample.Talk.Persistence.EventHandlers;
using ConferenceExample.Talk.Persistence.EventSubscriptions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace ConferenceExample.Talk.Persistence.UnitTests;

public class TalkEventSubscriptionsTests
{
    [Fact]
    public void Subscribe_RegistersAllTalkDomainEvents()
    {
        // Arrange
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

        // Act
        TalkEventSubscriptions.Subscribe(eventBus, scopeFactory);

        // Assert
        foreach (var eventType in expectedEventTypes)
        {
            eventBus.Received(1).Subscribe(eventType, Arg.Any<Func<StoredEvent, Task>>());
        }
    }

    [Fact]
    public void Subscribe_RegistersAllSpeakerDomainEvents()
    {
        // Arrange
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();

        var expectedEventTypes = new[]
        {
            "SpeakerProfileCreatedEvent",
            "SpeakerProfileUpdatedEvent",
        };

        // Act
        TalkEventSubscriptions.Subscribe(eventBus, scopeFactory);

        // Assert
        foreach (var eventType in expectedEventTypes)
        {
            eventBus.Received(1).Subscribe(eventType, Arg.Any<Func<StoredEvent, Task>>());
        }
    }

    [Fact]
    public void Subscribe_RegistersExactlySevenEventTypes()
    {
        // Arrange
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();

        // Act
        TalkEventSubscriptions.Subscribe(eventBus, scopeFactory);

        // Assert - 5 talk events + 2 speaker events
        eventBus.Received(7).Subscribe(Arg.Any<string>(), Arg.Any<Func<StoredEvent, Task>>());
    }

    [Fact]
    public async Task Subscribe_EventHandler_CreatesScopeAndCallsHandler()
    {
        // Arrange
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var handler = Substitute.For<TalkEventHandler>(
            Substitute.For<ReadModels.ITalkDocumentRepository>()
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

        TalkEventSubscriptions.Subscribe(eventBus, scopeFactory);

        var storedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
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
    public void Subscribe_CanBeCalledMultipleTimes()
    {
        // Arrange
        var eventBus = Substitute.For<IEventBus>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();

        // Act - call multiple times
        TalkEventSubscriptions.Subscribe(eventBus, scopeFactory);
        TalkEventSubscriptions.Subscribe(eventBus, scopeFactory);

        // Assert - each call should register handlers (7 events × 2 calls = 14)
        eventBus.Received(14).Subscribe(Arg.Any<string>(), Arg.Any<Func<StoredEvent, Task>>());
    }
}
