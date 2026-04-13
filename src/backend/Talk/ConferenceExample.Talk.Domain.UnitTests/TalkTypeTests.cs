namespace ConferenceExample.Talk.Domain.UnitTests;

using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.TalkManagement;

public class TalkTypeTests
{
    [Fact]
    public void Constructor_ValidParameters_SetsProperties()
    {
        // Arrange
        var id = new TalkTypeId(GuidV7.NewGuid());
        var name = new Text("Workshop");

        // Act
        var talkType = new TalkType(id, name);

        // Assert
        Assert.Equal(id, talkType.Id);
        Assert.Equal(name, talkType.Name);
    }

    [Fact]
    public void Constructor_DifferentNames_CreatesDistinctInstances()
    {
        // Arrange
        var id1 = new TalkTypeId(GuidV7.NewGuid());
        var id2 = new TalkTypeId(GuidV7.NewGuid());
        var workshopName = new Text("Workshop");
        var presentationName = new Text("Presentation");

        // Act
        var workshop = new TalkType(id1, workshopName);
        var presentation = new TalkType(id2, presentationName);

        // Assert
        Assert.NotEqual(workshop.Id, presentation.Id);
        Assert.NotEqual(workshop.Name, presentation.Name);
    }

    [Fact]
    public void Constructor_SameId_DifferentNames_CreatesDistinctInstances()
    {
        // Arrange
        var id = new TalkTypeId(GuidV7.NewGuid());
        var name1 = new Text("Lightning Talk");
        var name2 = new Text("Keynote");

        // Act
        var talkType1 = new TalkType(id, name1);
        var talkType2 = new TalkType(id, name2);

        // Assert
        Assert.Equal(id, talkType1.Id);
        Assert.Equal(id, talkType2.Id);
        Assert.NotEqual(talkType1.Name, talkType2.Name);
    }
}
