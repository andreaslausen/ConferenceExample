using ConferenceExample.Conference.Application.Queries;
using ConferenceExample.Conference.Domain;
using ConferenceExample.Conference.Domain.Repositories;
using NSubstitute;

namespace ConferenceExample.Conference.Application.UnitTests;

public class ConferenceServiceTests
{
    [Fact]
    public async Task CreateConference_ValidDto_CallsRepositorySave()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var queryHandler = Substitute.For<IGetConferenceSessionsQueryHandler>();
        var service = new ConferenceService(repository, queryHandler);

        // Act
        await service.CreateConference(CreateDto());

        // Assert
        await repository.Received(1).Save(Arg.Any<Domain.Conference>());
    }

    [Fact]
    public async Task CreateConference_ValidDto_CreatesConferenceWithCorrectProperties()
    {
        // Arrange
        Domain.Conference? savedConference = null;
        var repository = Substitute.For<IConferenceRepository>();
        repository
            .Save(Arg.Do<Domain.Conference>(c => savedConference = c))
            .Returns(Task.CompletedTask);
        var queryHandler = Substitute.For<IGetConferenceSessionsQueryHandler>();
        var service = new ConferenceService(repository, queryHandler);

        var dto = CreateDto();

        // Act
        await service.CreateConference(dto);

        // Assert
        Assert.NotNull(savedConference);
        Assert.Equal(dto.Name, savedConference.Name.Value);
        Assert.Equal(dto.Start, savedConference.ConferenceTime.Start);
        Assert.Equal(dto.End, savedConference.ConferenceTime.End);
        Assert.Equal(dto.LocationName, savedConference.Location.Name.Value);
        Assert.Equal(dto.Street, savedConference.Location.Address.Street);
        Assert.Equal(dto.City, savedConference.Location.Address.City);
    }

    private static CreateConferenceDto CreateDto() =>
        new()
        {
            Name = "DotNet Conf",
            Start = new DateTimeOffset(2026, 9, 1, 9, 0, 0, TimeSpan.Zero),
            End = new DateTimeOffset(2026, 9, 3, 18, 0, 0, TimeSpan.Zero),
            LocationName = "Convention Center",
            Street = "Main Street 1",
            City = "Berlin",
            State = "Berlin",
            PostalCode = "10115",
            Country = "Germany",
        };
}
