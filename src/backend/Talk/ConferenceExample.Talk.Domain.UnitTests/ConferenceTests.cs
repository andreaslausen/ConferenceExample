namespace ConferenceExample.Talk.Domain.UnitTests;

using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.TalkManagement;
using Xunit;
using ConferenceEntity = ConferenceExample.Talk.Domain.TalkManagement.Conference;

public class ConferenceTests
{
    [Fact]
    public void FromEvents_ValidParameters_InitializesProperties()
    {
        // Arrange
        var id = new ConferenceId(GuidV7.NewGuid());

        // Act
        var conference = ConferenceEntity.FromEvents(id, "CallForSpeakers");

        // Assert
        Assert.Equal(id, conference.Id);
        Assert.Equal("CallForSpeakers", conference.Status);
    }

    [Fact]
    public void CanAcceptTalkSubmissions_StatusIsCallForSpeakers_ReturnsTrue()
    {
        // Arrange
        var conference = ConferenceEntity.FromEvents(
            new ConferenceId(GuidV7.NewGuid()),
            "CallForSpeakers"
        );

        // Act & Assert
        Assert.True(conference.CanAcceptTalkSubmissions());
    }

    [Fact]
    public void CanAcceptTalkSubmissions_StatusIsNotCallForSpeakers_ReturnsFalse()
    {
        // Arrange
        var conference = ConferenceEntity.FromEvents(new ConferenceId(GuidV7.NewGuid()), "Draft");

        // Act & Assert
        Assert.False(conference.CanAcceptTalkSubmissions());
    }
}
