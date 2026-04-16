namespace ConferenceExample.Conference.Domain.UnitTests;

using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.ConferenceManagement.Events;
using ConferenceExample.Conference.Domain.RoomManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Domain.TalkManagement;
using ConferenceExample.Conference.Domain.TalkManagement.Events;
using Xunit;
using ConferenceAggregate = ConferenceExample.Conference.Domain.ConferenceManagement.Conference;

public class ConferenceTests
{
    [Fact]
    public void Create_ValidParameters_InitializesProperties()
    {
        // Arrange & Act
        var conference = CreateValidConference();

        // Assert
        Assert.NotNull(conference.Id);
        Assert.Equal("Test Conference", conference.Name.Value);
        Assert.Equal("Test Venue", conference.Location.Name.Value);
        Assert.Equal(ConferenceStatus.Draft, conference.Status);
        Assert.Empty(conference.Talks);
    }

    [Fact]
    public void Create_RaisesConferenceCreatedEvent()
    {
        // Act
        var conference = CreateValidConference();

        // Assert
        var events = conference.GetUncommittedEvents();
        var createdEvent = Assert.Single(events);
        Assert.IsType<ConferenceCreatedEvent>(createdEvent);
    }

    [Fact]
    public void Rename_ValidName_UpdatesName()
    {
        // Arrange
        var conference = CreateValidConference();
        var newName = new Text("Renamed Conference");

        // Act
        conference.Rename(newName);

        // Assert
        Assert.Equal(newName, conference.Name);
    }

    [Fact]
    public void Rename_RaisesConferenceRenamedEvent()
    {
        // Arrange
        var conference = CreateValidConference();

        // Act
        conference.Rename(new Text("Renamed Conference"));

        // Assert
        var events = conference.GetUncommittedEvents();
        Assert.Equal(2, events.Count);
        Assert.IsType<ConferenceRenamedEvent>(events[1]);
    }

    [Fact]
    public void ChangeStatus_ValidStatus_UpdatesStatus()
    {
        // Arrange
        var conference = CreateValidConference();

        // Act
        conference.ChangeStatus(ConferenceStatus.CallForSpeakers);

        // Assert
        Assert.Equal(ConferenceStatus.CallForSpeakers, conference.Status);
    }

    [Fact]
    public void ChangeStatus_RaisesConferenceStatusChangedEvent()
    {
        // Arrange
        var conference = CreateValidConference();

        // Act
        conference.ChangeStatus(ConferenceStatus.CallForSpeakers);

        // Assert
        var events = conference.GetUncommittedEvents();
        Assert.Equal(2, events.Count);
        Assert.IsType<ConferenceStatusChangedEvent>(events[1]);
    }

    [Fact]
    public void ChangeStatus_MultipleChanges_UpdatesStatusCorrectly()
    {
        // Arrange
        var conference = CreateValidConference();

        // Act
        conference.ChangeStatus(ConferenceStatus.CallForSpeakers);
        conference.ChangeStatus(ConferenceStatus.CallForSpeakersClosed);
        conference.ChangeStatus(ConferenceStatus.ProgramPublished);

        // Assert
        Assert.Equal(ConferenceStatus.ProgramPublished, conference.Status);
    }

    [Fact]
    public void SubmitTalk_AddsTalkWithSubmittedStatus()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());

        // Act
        conference.SubmitTalk(talkId);

        // Assert
        var talk = Assert.Single(conference.Talks);
        Assert.Equal(talkId, talk.Id);
        Assert.Equal(TalkStatus.Submitted, talk.Status);
    }

    [Fact]
    public void SubmitTalk_RaisesTalkSubmittedToConferenceEvent()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());

        // Act
        conference.SubmitTalk(talkId);

        // Assert
        var events = conference.GetUncommittedEvents();
        Assert.Equal(2, events.Count);
        Assert.IsType<TalkSubmittedToConferenceEvent>(events[1]);
    }

    [Fact]
    public void AcceptTalk_ChangesStatusToAccepted()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId);

        // Act
        conference.AcceptTalk(talkId);

        // Assert
        Assert.Equal(TalkStatus.Accepted, conference.Talks[0].Status);
    }

    [Fact]
    public void RejectTalk_ChangesStatusToRejected()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId);

        // Act
        conference.RejectTalk(talkId);

        // Assert
        Assert.Equal(TalkStatus.Rejected, conference.Talks[0].Status);
    }

    [Fact]
    public void ScheduleTalk_SetsSlot()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId);
        conference.AcceptTalk(talkId);
        var slot = new Time(DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2));

        // Act
        conference.ScheduleTalk(talkId, slot);

        // Assert
        Assert.Equal(slot, conference.Talks[0].Slot);
    }

    [Fact]
    public void AssignTalkToRoom_SetsRoom()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId);
        conference.AcceptTalk(talkId);
        var room = new Room(new RoomId(GuidV7.NewGuid()), new Text("Room A"));

        // Act
        conference.AssignTalkToRoom(talkId, room);

        // Assert
        Assert.NotNull(conference.Talks[0].Room);
        Assert.Equal(room.Id, conference.Talks[0].Room!.Id);
        Assert.Equal(room.Name, conference.Talks[0].Room!.Name);
    }

    [Fact]
    public void AcceptTalk_RaisesTalkAcceptedEvent()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId);
        conference.ClearUncommittedEvents();

        // Act
        conference.AcceptTalk(talkId);

        // Assert
        var events = conference.GetUncommittedEvents();
        var single = Assert.Single(events);
        Assert.IsType<TalkAcceptedEvent>(single);
    }

    [Fact]
    public void RejectTalk_RaisesTalkRejectedEvent()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId);
        conference.ClearUncommittedEvents();

        // Act
        conference.RejectTalk(talkId);

        // Assert
        var events = conference.GetUncommittedEvents();
        var single = Assert.Single(events);
        Assert.IsType<TalkRejectedEvent>(single);
    }

    [Fact]
    public void ScheduleTalk_RaisesTalkScheduledEvent()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId);
        conference.AcceptTalk(talkId);
        conference.ClearUncommittedEvents();
        var slot = new Time(DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2));

        // Act
        conference.ScheduleTalk(talkId, slot);

        // Assert
        var events = conference.GetUncommittedEvents();
        var single = Assert.Single(events);
        Assert.IsType<TalkScheduledEvent>(single);
    }

    [Fact]
    public void AssignTalkToRoom_RaisesTalkAssignedToRoomEvent()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId);
        conference.AcceptTalk(talkId);
        conference.ClearUncommittedEvents();
        var room = new Room(new RoomId(GuidV7.NewGuid()), new Text("Room A"));

        // Act
        conference.AssignTalkToRoom(talkId, room);

        // Assert
        var events = conference.GetUncommittedEvents();
        var single = Assert.Single(events);
        Assert.IsType<TalkAssignedToRoomEvent>(single);
    }

    [Fact]
    public void ReplayEvents_RestoresState()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId);
        conference.AcceptTalk(talkId);
        conference.Rename(new Text("Replayed Conference"));
        var events = conference.GetUncommittedEvents().ToList();

        // Act
        var replayedConference = ConferenceAggregate.LoadFromHistory(events);

        // Assert
        Assert.Equal(conference.Id, replayedConference.Id);
        Assert.Equal(new Text("Replayed Conference"), replayedConference.Name);
        Assert.Equal(conference.Location, replayedConference.Location);
        Assert.Equal(conference.ConferenceTime, replayedConference.ConferenceTime);
        var talk = Assert.Single(replayedConference.Talks);
        Assert.Equal(talkId, talk.Id);
        Assert.Equal(TalkStatus.Accepted, talk.Status);
        Assert.Equal(3, replayedConference.Version);
    }

    [Fact]
    public void DefineTalkType_ValidParameters_AddsTalkType()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkTypeId = new TalkTypeId(GuidV7.NewGuid());
        var name = new Text("Workshop");

        // Act
        conference.DefineTalkType(talkTypeId, name);

        // Assert
        var talkType = Assert.Single(conference.TalkTypes);
        Assert.Equal(talkTypeId, talkType.Id);
        Assert.Equal(name, talkType.Name);
    }

    [Fact]
    public void DefineTalkType_RaisesTalkTypeDefinedEvent()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkTypeId = new TalkTypeId(GuidV7.NewGuid());
        var name = new Text("Workshop");

        // Act
        conference.DefineTalkType(talkTypeId, name);

        // Assert
        var events = conference.GetUncommittedEvents();
        Assert.Equal(2, events.Count);
        Assert.IsType<TalkTypeDefinedEvent>(events[1]);
    }

    [Fact]
    public void DefineTalkType_DuplicateName_ThrowsException()
    {
        // Arrange
        var conference = CreateValidConference();
        var name = new Text("Workshop");
        conference.DefineTalkType(new TalkTypeId(GuidV7.NewGuid()), name);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            conference.DefineTalkType(new TalkTypeId(GuidV7.NewGuid()), name)
        );
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public void DefineTalkType_DuplicateNameDifferentCase_ThrowsException()
    {
        // Arrange
        var conference = CreateValidConference();
        conference.DefineTalkType(new TalkTypeId(GuidV7.NewGuid()), new Text("Workshop"));

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            conference.DefineTalkType(new TalkTypeId(GuidV7.NewGuid()), new Text("WORKSHOP"))
        );
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public void RemoveTalkType_ExistingTalkType_RemovesTalkType()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkTypeId = new TalkTypeId(GuidV7.NewGuid());
        conference.DefineTalkType(talkTypeId, new Text("Workshop"));

        // Act
        conference.RemoveTalkType(talkTypeId);

        // Assert
        Assert.Empty(conference.TalkTypes);
    }

    [Fact]
    public void RemoveTalkType_RaisesTalkTypeRemovedEvent()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkTypeId = new TalkTypeId(GuidV7.NewGuid());
        conference.DefineTalkType(talkTypeId, new Text("Workshop"));

        // Act
        conference.RemoveTalkType(talkTypeId);

        // Assert
        var events = conference.GetUncommittedEvents();
        Assert.Equal(3, events.Count);
        Assert.IsType<TalkTypeRemovedEvent>(events[2]);
    }

    [Fact]
    public void RemoveTalkType_NonExistingTalkType_ThrowsException()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkTypeId = new TalkTypeId(GuidV7.NewGuid());

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            conference.RemoveTalkType(talkTypeId)
        );
        Assert.Contains("does not exist", exception.Message);
    }

    [Fact]
    public void LoadFromHistory_WithTalkTypeEvents_ReconstructsCorrectly()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkTypeId = new TalkTypeId(GuidV7.NewGuid());
        conference.DefineTalkType(talkTypeId, new Text("Workshop"));
        var events = conference.GetUncommittedEvents();

        // Act
        var replayedConference = ConferenceAggregate.LoadFromHistory(events);

        // Assert
        var talkType = Assert.Single(replayedConference.TalkTypes);
        Assert.Equal(talkTypeId, talkType.Id);
        Assert.Equal(new Text("Workshop"), talkType.Name);
    }

    private static ConferenceAggregate CreateValidConference()
    {
        var id = new ConferenceId(GuidV7.NewGuid());
        var name = new Text("Test Conference");
        var time = new Time(DateTimeOffset.UtcNow.AddDays(30), DateTimeOffset.UtcNow.AddDays(32));
        var location = new Location(
            new Text("Test Venue"),
            new Address("123 Main St", "Springfield", "IL", "62701", "US")
        );
        var organizerId = new OrganizerId(GuidV7.NewGuid());

        return ConferenceAggregate.Create(id, name, time, location, organizerId);
    }
}
