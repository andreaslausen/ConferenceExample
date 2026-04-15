using ConferenceExample.Conference.Application.AssignTalkToRoom;
using ConferenceExample.Conference.Application.ChangeConferenceStatus;
using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.Conference.Application.GetAllConferences;
using ConferenceExample.Conference.Application.GetConferenceById;
using ConferenceExample.Conference.Application.GetConferenceSessions;
using ConferenceExample.Conference.Application.GetConferenceTalks;
using ConferenceExample.Conference.Application.RenameConference;
using ConferenceExample.Conference.Application.ScheduleTalk;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Conference.Application.UnitTests;

public class DtoTests
{
    [Fact]
    public void AssignTalkToRoomDto_CanBeCreated()
    {
        // Arrange & Act
        var dto = new AssignTalkToRoomDto { RoomId = Guid.NewGuid(), RoomName = "Room A" };

        // Assert
        Assert.NotEqual(Guid.Empty, dto.RoomId);
        Assert.Equal("Room A", dto.RoomName);
    }

    [Fact]
    public void ChangeConferenceStatusDto_CanBeCreated()
    {
        // Arrange & Act
        var dto = new ChangeConferenceStatusDto
        {
            Status = Domain.ConferenceManagement.ConferenceStatus.CallForSpeakers,
        };

        // Assert
        Assert.Equal(Domain.ConferenceManagement.ConferenceStatus.CallForSpeakers, dto.Status);
    }

    [Fact]
    public void ScheduleTalkDto_CanBeCreated()
    {
        // Arrange
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var end = start.AddHours(1);

        // Act
        var dto = new ScheduleTalkDto { Start = start, End = end };

        // Assert
        Assert.Equal(start, dto.Start);
        Assert.Equal(end, dto.End);
    }

    [Fact]
    public void ConferenceCreatedDto_AllProperties_AreAccessible()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test Conference";
        var start = DateTimeOffset.UtcNow;
        var end = start.AddDays(2);
        var locationName = "Convention Center";

        // Act
        var dto = new ConferenceCreatedDto(id, name, start, end, locationName);

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal(name, dto.Name);
        Assert.Equal(start, dto.Start);
        Assert.Equal(end, dto.End);
        Assert.Equal(locationName, dto.LocationName);
    }

    [Fact]
    public void GetConferenceSessionDto_AllProperties_AreAccessible()
    {
        // Arrange
        var id = Guid.NewGuid();
        var status = "Accepted";
        var slotStart = DateTimeOffset.UtcNow;
        var slotEnd = slotStart.AddHours(1);
        var roomId = Guid.NewGuid();
        var roomName = "Room A";

        // Act
        var dto = new GetConferenceSessionDto(id, status, slotStart, slotEnd, roomId, roomName);

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal(status, dto.Status);
        Assert.Equal(slotStart, dto.SlotStart);
        Assert.Equal(slotEnd, dto.SlotEnd);
        Assert.Equal(roomId, dto.RoomId);
        Assert.Equal(roomName, dto.RoomName);
    }

    [Fact]
    public void GetConferenceSessionDto_WithDeconstruction_WorksCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var status = "Accepted";
        var slotStart = DateTimeOffset.UtcNow;
        var slotEnd = slotStart.AddHours(1);
        var roomId = Guid.NewGuid();
        var roomName = "Room A";
        var dto = new GetConferenceSessionDto(id, status, slotStart, slotEnd, roomId, roomName);

        // Act
        var (dId, dStatus, dSlotStart, dSlotEnd, dRoomId, dRoomName) = dto;

        // Assert
        Assert.Equal(id, dId);
        Assert.Equal(status, dStatus);
        Assert.Equal(slotStart, dSlotStart);
        Assert.Equal(slotEnd, dSlotEnd);
        Assert.Equal(roomId, dRoomId);
        Assert.Equal(roomName, dRoomName);
    }

    [Fact]
    public void GetConferenceTalksDto_AllProperties_AreAccessible()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Talk Title";
        var abstractText = "Talk Abstract";
        var speakerId = Guid.NewGuid();
        var status = "Accepted";
        var tags = new List<string> { "tag1", "tag2" };
        var talkTypeId = Guid.NewGuid();

        // Act
        var dto = new GetConferenceTalksDto(
            id,
            title,
            abstractText,
            speakerId,
            status,
            tags,
            talkTypeId
        );

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal(title, dto.Title);
        Assert.Equal(abstractText, dto.Abstract);
        Assert.Equal(speakerId, dto.SpeakerId);
        Assert.Equal(status, dto.Status);
        Assert.Equal(tags, dto.Tags);
        Assert.Equal(talkTypeId, dto.TalkTypeId);
    }
}
