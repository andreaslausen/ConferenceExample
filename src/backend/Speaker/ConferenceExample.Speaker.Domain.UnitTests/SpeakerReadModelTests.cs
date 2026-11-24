using ConferenceExample.Speaker.Domain.SpeakerManagement;

namespace ConferenceExample.Speaker.Domain.UnitTests;

public class SpeakerReadModelTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        var id = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();

        var readModel = new SpeakerReadModel(id, userId, "Jane", "Doe", "Speaker bio");

        Assert.Equal(id, readModel.Id);
        Assert.Equal(userId, readModel.UserId);
        Assert.Equal("Jane", readModel.FirstName);
        Assert.Equal("Doe", readModel.LastName);
        Assert.Equal("Speaker bio", readModel.Biography);
    }

    [Fact]
    public void Equality_SameValues_ReturnsTrue()
    {
        var id = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();

        Assert.Equal(
            new SpeakerReadModel(id, userId, "Jane", "Doe", "Speaker bio"),
            new SpeakerReadModel(id, userId, "Jane", "Doe", "Speaker bio")
        );
    }
}
