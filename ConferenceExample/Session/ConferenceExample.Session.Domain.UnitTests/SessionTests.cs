namespace ConferenceExample.Session.Domain.UnitTests;

using Xunit;
using System;
using System.Collections.Generic;
using ConferenceExample.Session.Domain.ValueObjects;
using ConferenceExample.Session.Domain.ValueObjects.Ids;

public class SessionTests
{
    [Fact]
    public void Constructor_ValidParameters_InitializesProperties()
    {
        // Arrange
        var id = new SessionId(1);
        var title = new SessionTitle("Test Title");
        var speakerId = new SpeakerId(2);
        var tags = new List<SessionTag> { new SessionTag("Tag1"), new SessionTag("Tag2") };
        var sessionTypeId = new SessionTypeId(3);
        var @abstract = new Abstract("Test Abstract");
        var conferenceId = new ConferenceId(4);

        // Act
        var session = new Entities.Session(id, title, speakerId, tags, sessionTypeId, @abstract, conferenceId);

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
    public void Constructor_NullTags_ThrowsArgumentNullException()
    {
        // Arrange
        var id = new SessionId(1);
        var title = new SessionTitle("Test Title");
        var speakerId = new SpeakerId(2);
        var sessionTypeId = new SessionTypeId(3);
        var @abstract = new Abstract("Test Abstract");
        var conferenceId = new ConferenceId(4);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Entities.Session(id, title, speakerId, null!, sessionTypeId, @abstract, conferenceId));
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

    private Entities.Session CreateValidSession()
    {
        var id = new SessionId(1);
        var title = new SessionTitle("Test Title");
        var speakerId = new SpeakerId(2);
        var tags = new List<SessionTag> { new SessionTag("Tag1"), new SessionTag("Tag2") };
        var sessionTypeId = new SessionTypeId(3);
        var @abstract = new Abstract("Test Abstract");
        var conferenceId = new ConferenceId(4);

        return new Entities.Session(id, title, speakerId, tags, sessionTypeId, @abstract, conferenceId);
    }
}
