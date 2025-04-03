using ConferenceExample.Session.Domain.Entities;
using ConferenceExample.Session.Domain.ValueObjects;
using ConferenceExample.Session.Domain.ValueObjects.Ids;
using Shouldly;
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
        session.Title.ShouldBe(newTitle);
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
        session.Abstract.ShouldBe(newAbstract);
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
        session.Tags.ShouldContain(newTag);
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
        session.Tags.ShouldNotContain(tagToRemove);
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
        session.Id.ShouldBe(sessionId);
        session.Title.ShouldBe(title);
        session.SpeakerId.ShouldBe(speakerId);
        session.Tags.ShouldBe(tags);
        session.SessionTypeId.ShouldBe(sessionTypeId);
        session.Abstract.ShouldBe(@abstract);
        session.ConferenceId.ShouldBe(conferenceId);
        session.Status.ShouldBe(status);
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