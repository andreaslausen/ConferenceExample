using ConferenceExample.Talk.Application.CreateSpeakerProfile;
using ConferenceExample.Talk.Application.EditTalk;
using ConferenceExample.Talk.Application.GetMyTalks;
using ConferenceExample.Talk.Application.SubmitTalk;

namespace ConferenceExample.Talk.Application.UnitTests;

public class DtoTests
{
    [Fact]
    public void SubmitTalkDto_CanBeCreated()
    {
        // Arrange & Act
        var dto = new SubmitTalkDto
        {
            Title = "Test Title",
            Abstract = "Test Abstract",
            ConferenceId = Guid.CreateVersion7(),
            Tags = new List<string> { "tag1", "tag2" },
            TalkTypeId = Guid.CreateVersion7(),
        };

        // Assert
        Assert.Equal("Test Title", dto.Title);
        Assert.Equal("Test Abstract", dto.Abstract);
        Assert.NotEqual(Guid.Empty, dto.ConferenceId);
        Assert.Equal(2, dto.Tags.Count);
        Assert.NotEqual(Guid.Empty, dto.TalkTypeId);
    }

    [Fact]
    public void SubmitTalkCommand_CanBeCreated()
    {
        // Arrange
        var title = "Test Title";
        var @abstract = "Test Abstract";
        var conferenceId = Guid.CreateVersion7();
        var tags = new List<string> { "tag1" };
        var talkTypeId = Guid.CreateVersion7();

        // Act
        var command = new SubmitTalkCommand(title, @abstract, conferenceId, tags, talkTypeId);

        // Assert
        Assert.Equal(title, command.Title);
        Assert.Equal(@abstract, command.Abstract);
        Assert.Equal(conferenceId, command.ConferenceId);
        Assert.Equal(tags, command.Tags);
        Assert.Equal(talkTypeId, command.TalkTypeId);
    }

    [Fact]
    public void EditTalkDto_CanBeCreated()
    {
        // Arrange
        var title = "Updated Title";
        var @abstract = "Updated Abstract";
        var tags = new List<string> { "tag1", "tag2" };

        // Act
        var dto = new EditTalkDto(title, @abstract, tags);

        // Assert
        Assert.Equal(title, dto.Title);
        Assert.Equal(@abstract, dto.Abstract);
        Assert.Equal(tags, dto.Tags);
    }

    [Fact]
    public void EditTalkCommand_CanBeCreated()
    {
        // Arrange
        var talkId = Guid.CreateVersion7();
        var title = "Updated Title";
        var @abstract = "Updated Abstract";
        var tags = new List<string> { "tag1" };

        // Act
        var command = new EditTalkCommand(talkId, title, @abstract, tags);

        // Assert
        Assert.Equal(talkId, command.TalkId);
        Assert.Equal(title, command.Title);
        Assert.Equal(@abstract, command.Abstract);
        Assert.Equal(tags, command.Tags);
    }

    [Fact]
    public void GetMyTalksDto_CanBeCreated()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var title = "Test Title";
        var @abstract = "Test Abstract";
        var conferenceId = Guid.CreateVersion7();
        var status = "Submitted";
        var tags = new List<string> { "tag1", "tag2" };

        // Act
        var dto = new GetMyTalksDto(id, title, @abstract, conferenceId, status, tags);

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal(title, dto.Title);
        Assert.Equal(@abstract, dto.Abstract);
        Assert.Equal(conferenceId, dto.ConferenceId);
        Assert.Equal(status, dto.Status);
        Assert.Equal(tags, dto.Tags);
    }

    [Fact]
    public void GetMyTalksQuery_CanBeCreated()
    {
        // Act
        var query = new GetMyTalksQuery();

        // Assert
        Assert.NotNull(query);
    }

    [Fact]
    public void EditTalkDto_RecordEquality()
    {
        // Arrange
        var tags = new List<string> { "tag1" };
        var dto1 = new EditTalkDto("Title", "Abstract", tags);
        var dto2 = new EditTalkDto("Title", "Abstract", tags);

        // Act & Assert
        Assert.Equal(dto1, dto2);
    }

    [Fact]
    public void GetMyTalksDto_RecordEquality()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var tags = new List<string> { "tag1" };
        var dto1 = new GetMyTalksDto(id, "Title", "Abstract", Guid.CreateVersion7(), "Status", tags);
        var dto2 = new GetMyTalksDto(id, "Title", "Abstract", dto1.ConferenceId, "Status", tags);

        // Act & Assert
        Assert.Equal(dto1, dto2);
    }

    [Fact]
    public void SpeakerProfileCreatedDto_CanBeCreated()
    {
        // Arrange
        var id = Guid.CreateVersion7();

        // Act
        var dto = new SpeakerProfileCreatedDto(id);

        // Assert
        Assert.Equal(id, dto.Id);
    }
}
