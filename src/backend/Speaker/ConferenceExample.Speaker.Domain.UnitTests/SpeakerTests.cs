using ConferenceExample.Speaker.Domain.SharedKernel;
using ConferenceExample.Speaker.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Speaker.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Speaker.Domain.SpeakerManagement;
using ConferenceExample.Speaker.Domain.SpeakerManagement.Events;
using SpeakerAggregate = ConferenceExample.Speaker.Domain.SpeakerManagement.Speaker;

namespace ConferenceExample.Speaker.Domain.UnitTests;

public class SpeakerTests
{
    [Fact]
    public void Create_ValidParameters_InitializesProperties()
    {
        // Arrange
        var id = new SpeakerId(GuidV7.NewGuid());
        var userId = new UserId(GuidV7.NewGuid());
        var name = new Name("Jane", "Doe");
        var biography = new SpeakerBiography("Speaker bio");

        // Act
        var speaker = SpeakerAggregate.Create(id, userId, name, biography);

        // Assert
        Assert.Equal(id, speaker.Id);
        Assert.Equal(userId, speaker.UserId);
        Assert.Equal(name, speaker.Name);
        Assert.Equal(biography, speaker.Biography);
    }

    [Fact]
    public void Create_RaisesSpeakerProfileCreatedEventCarryingTheOwningAccount()
    {
        // Arrange
        var id = new SpeakerId(GuidV7.NewGuid());
        var userId = new UserId(GuidV7.NewGuid());

        // Act
        var speaker = SpeakerAggregate.Create(
            id,
            userId,
            new Name("Jane", "Doe"),
            new SpeakerBiography("Speaker bio")
        );

        // Assert
        var @event = Assert.Single(speaker.GetUncommittedEvents());
        var created = Assert.IsType<SpeakerProfileCreatedEvent>(@event);
        Assert.Equal(id.Value.Value, created.AggregateId);
        Assert.Equal(userId.Value.Value, created.UserId);
        Assert.Equal("Jane", created.FirstName);
        Assert.Equal("Doe", created.LastName);
        Assert.Equal("Speaker bio", created.Biography);
    }

    [Fact]
    public void UpdateProfile_ChangesNameAndBiography()
    {
        // Arrange
        var speaker = CreateSpeaker();
        speaker.ClearUncommittedEvents();

        // Act
        speaker.UpdateProfile(new Name("John", "Smith"), new SpeakerBiography("Updated bio"));

        // Assert
        Assert.Equal(new Name("John", "Smith"), speaker.Name);
        Assert.Equal(new SpeakerBiography("Updated bio"), speaker.Biography);
    }

    [Fact]
    public void UpdateProfile_RaisesSpeakerProfileUpdatedEvent()
    {
        // Arrange
        var speaker = CreateSpeaker();
        speaker.ClearUncommittedEvents();

        // Act
        speaker.UpdateProfile(new Name("John", "Smith"), new SpeakerBiography("Updated bio"));

        // Assert
        var @event = Assert.Single(speaker.GetUncommittedEvents());
        var updated = Assert.IsType<SpeakerProfileUpdatedEvent>(@event);
        Assert.Equal(speaker.Id.Value.Value, updated.AggregateId);
        Assert.Equal("John", updated.FirstName);
        Assert.Equal("Smith", updated.LastName);
        Assert.Equal("Updated bio", updated.Biography);
    }

    [Fact]
    public void UpdateProfile_LeavesTheOwningAccountUntouched()
    {
        // Arrange
        var userId = new UserId(GuidV7.NewGuid());
        var speaker = SpeakerAggregate.Create(
            new SpeakerId(GuidV7.NewGuid()),
            userId,
            new Name("Jane", "Doe"),
            new SpeakerBiography("Speaker bio")
        );

        // Act
        speaker.UpdateProfile(new Name("John", "Smith"), new SpeakerBiography("Updated bio"));

        // Assert
        Assert.Equal(userId, speaker.UserId);
    }

    [Fact]
    public void LoadFromHistory_ReplaysEventsAndCountsVersion()
    {
        // Arrange
        var speakerId = GuidV7.NewGuid();
        var userId = GuidV7.NewGuid();
        var events = new List<IDomainEvent>
        {
            new SpeakerProfileCreatedEvent(
                speakerId,
                DateTimeOffset.UtcNow,
                userId,
                "Jane",
                "Doe",
                "Speaker bio"
            ),
            new SpeakerProfileUpdatedEvent(
                speakerId,
                DateTimeOffset.UtcNow,
                "John",
                "Smith",
                "Updated bio"
            ),
        };

        // Act
        var speaker = SpeakerAggregate.LoadFromHistory(events);

        // Assert
        Assert.Equal(new SpeakerId(speakerId), speaker.Id);
        Assert.Equal(new UserId(userId), speaker.UserId);
        Assert.Equal(new Name("John", "Smith"), speaker.Name);
        Assert.Equal(new SpeakerBiography("Updated bio"), speaker.Biography);
        Assert.Equal(1, speaker.Version);
        Assert.Empty(speaker.GetUncommittedEvents());
    }

    [Fact]
    public void LoadFromHistory_UnknownEvent_IsIgnored()
    {
        // Arrange
        var speakerId = GuidV7.NewGuid();
        var events = new List<IDomainEvent>
        {
            new SpeakerProfileCreatedEvent(
                speakerId,
                DateTimeOffset.UtcNow,
                GuidV7.NewGuid(),
                "Jane",
                "Doe",
                "Speaker bio"
            ),
            new UnknownEvent(speakerId, DateTimeOffset.UtcNow),
        };

        // Act
        var speaker = SpeakerAggregate.LoadFromHistory(events);

        // Assert
        Assert.Equal(new Name("Jane", "Doe"), speaker.Name);
    }

    private static SpeakerAggregate CreateSpeaker() =>
        SpeakerAggregate.Create(
            new SpeakerId(GuidV7.NewGuid()),
            new UserId(GuidV7.NewGuid()),
            new Name("Jane", "Doe"),
            new SpeakerBiography("Speaker bio")
        );

    private record UnknownEvent(Guid AggregateId, DateTimeOffset OccurredAt) : IDomainEvent;
}
