namespace ConferenceExample.Session.Domain.UnitTests;

using ConferenceExample.Session.Domain.Events;
using ConferenceExample.Session.Domain.ValueObjects;
using ConferenceExample.Session.Domain.ValueObjects.Ids;
using Xunit;

public class SessionTests
{
    [Fact]
    public void Submit_ValidParameters_InitializesProperties()
    {
        // Arrange
        var id = new SessionId(GuidV7.NewGuid());
        var title = new SessionTitle("Test Title");
        var speakerId = new SpeakerId(GuidV7.NewGuid());
        var tags = new List<SessionTag> { new("Tag1"), new("Tag2") };
        var sessionTypeId = new SessionTypeId(GuidV7.NewGuid());
        var @abstract = new Abstract("Test Abstract");
        var conferenceId = new ConferenceId(GuidV7.NewGuid());

        // Act
        var session = Entities.Session.Submit(
            id,
            title,
            speakerId,
            tags,
            sessionTypeId,
            @abstract,
            conferenceId
        );

        // Assert
        Assert.Equal(id, session.Id);
        Assert.Equal(title, session.Title);
        Assert.Equal(speakerId, session.SpeakerId);
        Assert.Equal(sessionTypeId, session.SessionTypeId);
        Assert.Equal(@abstract, session.Abstract);
        Assert.Equal(conferenceId, session.ConferenceId);
        Assert.Equal(SessionStatus.Submitted, session.Status);
        Assert.Equal(tags, session.Tags);
    }

    [Fact]
    public void Submit_NullTags_ThrowsArgumentNullException()
    {
        // Arrange
        var id = new SessionId(GuidV7.NewGuid());
        var title = new SessionTitle("Test Title");
        var speakerId = new SpeakerId(GuidV7.NewGuid());
        var sessionTypeId = new SessionTypeId(GuidV7.NewGuid());
        var @abstract = new Abstract("Test Abstract");
        var conferenceId = new ConferenceId(GuidV7.NewGuid());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            Entities.Session.Submit(
                id,
                title,
                speakerId,
                null!,
                sessionTypeId,
                @abstract,
                conferenceId
            )
        );
    }

    [Fact]
    public void Submit_RaisesSessionSubmittedEvent()
    {
        // Act
        var session = CreateValidSession();

        // Assert
        var events = session.GetUncommittedEvents();
        var submittedEvent = Assert.Single(events);
        Assert.IsType<SessionSubmittedEvent>(submittedEvent);
    }

    [Fact]
    public void EditTitle_ValidTitle_UpdatesTitle()
    {
        // Arrange
        var session = CreateValidSession();
        var newTitle = new SessionTitle("Updated Title");

        // Act
        session.EditTitle(newTitle);

        // Assert
        Assert.Equal(newTitle, session.Title);
    }

    [Fact]
    public void EditTitle_RaisesSessionTitleEditedEvent()
    {
        // Arrange
        var session = CreateValidSession();

        // Act
        session.EditTitle(new SessionTitle("Updated Title"));

        // Assert
        var events = session.GetUncommittedEvents();
        Assert.Equal(2, events.Count);
        Assert.IsType<SessionTitleEditedEvent>(events[1]);
    }

    [Fact]
    public void EditAbstract_ValidAbstract_UpdatesAbstract()
    {
        // Arrange
        var session = CreateValidSession();
        var newAbstract = new Abstract("Updated Abstract");

        // Act
        session.EditAbstract(newAbstract);

        // Assert
        Assert.Equal(newAbstract, session.Abstract);
    }

    [Fact]
    public void AddTag_ValidTag_AddsTagToSession()
    {
        // Arrange
        var session = CreateValidSession();
        var newTag = new SessionTag("NewTag");

        // Act
        session.AddTag(newTag);

        // Assert
        Assert.Contains(newTag, session.Tags);
    }

    [Fact]
    public void RemoveTag_ValidTag_RemovesTagFromSession()
    {
        // Arrange
        var session = CreateValidSession();
        var tagToRemove = session.Tags[0];

        // Act
        session.RemoveTag(tagToRemove);

        // Assert
        Assert.DoesNotContain(tagToRemove, session.Tags);
    }

    [Fact]
    public void EditAbstract_RaisesSessionAbstractEditedEvent()
    {
        // Arrange
        var session = CreateValidSession();

        // Act
        session.EditAbstract(new Abstract("Updated Abstract"));

        // Assert
        var events = session.GetUncommittedEvents();
        Assert.Equal(2, events.Count);
        Assert.IsType<SessionAbstractEditedEvent>(events[1]);
    }

    [Fact]
    public void AddTag_RaisesSessionTagAddedEvent()
    {
        // Arrange
        var session = CreateValidSession();

        // Act
        session.AddTag(new SessionTag("NewTag"));

        // Assert
        var events = session.GetUncommittedEvents();
        Assert.Equal(2, events.Count);
        Assert.IsType<SessionTagAddedEvent>(events[1]);
    }

    [Fact]
    public void RemoveTag_RaisesSessionTagRemovedEvent()
    {
        // Arrange
        var session = CreateValidSession();
        var tagToRemove = session.Tags[0];

        // Act
        session.RemoveTag(tagToRemove);

        // Assert
        var events = session.GetUncommittedEvents();
        Assert.Equal(2, events.Count);
        Assert.IsType<SessionTagRemovedEvent>(events[1]);
    }

    [Fact]
    public void ReplayEvents_RestoresState()
    {
        // Arrange
        var session = CreateValidSession();
        session.EditTitle(new SessionTitle("Replayed Title"));
        var events = session.GetUncommittedEvents().ToList();

        // Act
        var replayedSession = Entities.Session.LoadFromHistory(events);

        // Assert
        Assert.Equal(session.Id, replayedSession.Id);
        Assert.Equal(new SessionTitle("Replayed Title"), replayedSession.Title);
        Assert.Equal(session.SpeakerId, replayedSession.SpeakerId);
        Assert.Equal(session.Tags, replayedSession.Tags);
        Assert.Equal(1, replayedSession.Version);
    }

    private static Entities.Session CreateValidSession()
    {
        var id = new SessionId(GuidV7.NewGuid());
        var title = new SessionTitle("Test Title");
        var speakerId = new SpeakerId(GuidV7.NewGuid());
        var tags = new List<SessionTag> { new("Tag1"), new("Tag2") };
        var sessionTypeId = new SessionTypeId(GuidV7.NewGuid());
        var @abstract = new Abstract("Test Abstract");
        var conferenceId = new ConferenceId(GuidV7.NewGuid());

        return Entities.Session.Submit(
            id,
            title,
            speakerId,
            tags,
            sessionTypeId,
            @abstract,
            conferenceId
        );
    }
}
