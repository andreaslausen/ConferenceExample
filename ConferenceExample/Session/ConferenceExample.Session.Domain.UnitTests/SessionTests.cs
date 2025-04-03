using ConferenceExample.Session.Domain.Entities;
using ConferenceExample.Session.Domain.ValueObjects;
using ConferenceExample.Session.Domain.ValueObjects.Ids;
using Xunit;

namespace ConferenceExample.Session.Domain.UnitTests;

public class SessionTests
{
    [Fact]
    public void EditTitle_ShouldUpdateTitle()
    {
        // Arrange
        var session = CreateSession();
        var newTitle = new SessionTitle("New Title");

        // Act
        session.EditTitle(newTitle);

        // Assert
        Assert.Equal(newTitle, session.Title);
    }

    [Fact]
    public void EditAbstract_ShouldUpdateAbstract()
    {
        // Arrange
        var session = CreateSession();
        var newAbstract = new Abstract("New Abstract");

        // Act
        session.EditAbstract(newAbstract);

        // Assert
        Assert.Equal(newAbstract, session.Abstract);
    }

    [Fact]
    public void AddTag_ShouldAddTagToList()
    {
        // Arrange
        var session = CreateSession();
        var newTag = new SessionTag("New Tag");

        // Act
        session.AddTag(newTag);

        // Assert
        Assert.Contains(newTag, session.Tags);
    }

    [Fact]
    public void RemoveTag_ShouldRemoveTagFromList()
    {
        // Arrange
        var session = CreateSession();
        var tagToRemove = new SessionTag("Tag3");
        session.AddTag(tagToRemove);

        // Act
        session.RemoveTag(tagToRemove);

        // Assert
        Assert.DoesNotContain(tagToRemove, session.Tags);
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var sessionId = new SessionId(1);
        var title = new SessionTitle("Initial Title");
        var speakerId = new SpeakerId(5);
        var tags = new List<SessionTag> { new SessionTag("Tag1"), new SessionTag("Tag2") };
        var sessionTypeId = new SessionTypeId(6);
        var @abstract = new Abstract("Initial Abstract");
        var conferenceId = new ConferenceId(87);
        var status = SessionStatus.Submitted;

        // Act
        var session = new Entities.Session(sessionId, title, speakerId, tags, sessionTypeId, @abstract, conferenceId, status);

        // Assert
        Assert.Equal(sessionId, session.Id);
        Assert.Equal(title, session.Title);
        Assert.Equal(speakerId, session.SpeakerId);
        Assert.Equal(tags, session.Tags);
        Assert.Equal(sessionTypeId, session.SessionTypeId);
        Assert.Equal(@abstract, session.Abstract);
        Assert.Equal(conferenceId, session.ConferenceId);
        Assert.Equal(status, session.Status);
    }

    private Entities.Session CreateSession()
    {
        return new Entities.Session(
            new SessionId(1),
            new SessionTitle("Initial Title"),
            new SpeakerId(5),
            new List<SessionTag> { new SessionTag("Tag1"), new SessionTag("Tag2") },
            new SessionTypeId(6),
            new Abstract("Initial Abstract"),
            new ConferenceId(87)
        );
    }
}