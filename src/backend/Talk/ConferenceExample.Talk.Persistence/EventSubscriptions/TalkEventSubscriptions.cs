using ConferenceExample.EventStore;
using ConferenceExample.Talk.Persistence.EventHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Talk.Persistence.EventSubscriptions;

public static class TalkEventSubscriptions
{
    public static void Subscribe(IEventBus eventBus, IServiceScopeFactory scopeFactory)
    {
        var conferenceEvents = new[]
        {
            "ConferenceCreatedEvent",
            "ConferenceRenamedEvent",
            "ConferenceDetailsUpdatedEvent",
            "ConferenceStatusChangedEvent",
            "TalkTypeDefinedEvent",
            "TalkTypeRemovedEvent",
            "RoomAddedEvent",
            "RoomRemovedEvent",
            "TalkAcceptedEvent",
            "TalkRejectedEvent",
            "TalkScheduledEvent",
            "TalkAssignedToRoomEvent",
        };

        foreach (var eventType in conferenceEvents)
        {
            eventBus.Subscribe(
                eventType,
                async storedEvent =>
                {
                    using var scope = scopeFactory.CreateScope();
                    var handler =
                        scope.ServiceProvider.GetRequiredService<ConferenceEventReplicationHandler>();
                    await handler.ReplicateEvent(storedEvent);
                }
            );
        }

        var talkDomainEvents = new[]
        {
            "TalkSubmittedEvent",
            "TalkTitleEditedEvent",
            "TalkAbstractEditedEvent",
            "TalkTagAddedEvent",
            "TalkTagRemovedEvent",
        };

        foreach (var eventType in talkDomainEvents)
        {
            eventBus.Subscribe(
                eventType,
                async storedEvent =>
                {
                    using var scope = scopeFactory.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<TalkEventHandler>();
                    await handler.HandleTalkDomainEvent(storedEvent);
                }
            );
        }

        var speakerDomainEvents = new[]
        {
            "SpeakerProfileCreatedEvent",
            "SpeakerProfileUpdatedEvent",
        };

        foreach (var eventType in speakerDomainEvents)
        {
            eventBus.Subscribe(
                eventType,
                async storedEvent =>
                {
                    using var scope = scopeFactory.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<SpeakerEventHandler>();
                    await handler.HandleSpeakerDomainEvent(storedEvent);
                }
            );
        }
    }
}
