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
        var conference = ConferenceEntity.FromEvents(id);

        // Assert
        Assert.Equal(id, conference.Id);
    }
}
