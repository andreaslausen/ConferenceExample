using System.Net;
using System.Net.Http.Json;
using ConferenceExample.Authentication;
using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.Conference.Application.GetAllConferences;
using ConferenceExample.Conference.Application.GetConferenceById;
using ConferenceExample.Conference.Application.RenameConference;
using ConferenceExample.IntegrationTests.Infrastructure;

namespace ConferenceExample.IntegrationTests;

[Collection("IntegrationTests")]
public class ConferencesControllerTests : IntegrationTestBase
{
    public ConferencesControllerTests(IntegrationTestWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task CreateConference_WithValidData_ReturnsCreatedConference()
    {
        // Arrange
        var token = await GetAuthenticationToken(
            GetUniqueEmail("organizer"),
            "password123",
            UserRole.Organizer
        );
        SetAuthenticationToken(token);

        var createDto = new CreateConferenceDto
        {
            Name = "DotNet Conf 2026",
            Start = new DateTimeOffset(2026, 9, 15, 9, 0, 0, TimeSpan.Zero),
            End = new DateTimeOffset(2026, 9, 17, 18, 0, 0, TimeSpan.Zero),
            LocationName = "Berlin Congress Center",
            Street = "Alexanderplatz 1",
            City = "Berlin",
            State = "Berlin",
            PostalCode = "10178",
            Country = "Germany",
        };

        // Act
        var response = await PostAsync("/api/conferences", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await DeserializeResponse<ConferenceCreatedDto>(response);
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result!.Id);
        Assert.Equal("DotNet Conf 2026", result.Name);
        Assert.Equal(createDto.Start, result.Start);
        Assert.Equal(createDto.End, result.End);
        Assert.Equal("Berlin Congress Center", result.LocationName);
    }

    [Fact]
    public async Task CreateConference_WithoutAuthorization_ReturnsUnauthorized()
    {
        // Arrange
        var createDto = new CreateConferenceDto
        {
            Name = "Test Conference",
            Start = DateTimeOffset.UtcNow,
            End = DateTimeOffset.UtcNow.AddDays(2),
            LocationName = "Test Location",
            Street = "Test Street",
            City = "Test City",
            State = "Test State",
            PostalCode = "12345",
            Country = "Test Country",
        };

        // Act
        var response = await PostAsync("/api/conferences", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAllConferences_ReturnsEmptyList_WhenNoConferencesExist()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/conferences");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<GetAllConferencesDto>>();
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllConferences_ReturnsConferences_WhenConferencesExist()
    {
        // Arrange
        var token = await GetAuthenticationToken(
            GetUniqueEmail("organizer"),
            "password123",
            UserRole.Organizer
        );
        SetAuthenticationToken(token);

        var createDto = new CreateConferenceDto
        {
            Name = "Test Conference",
            Start = new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero),
            End = new DateTimeOffset(2026, 10, 3, 18, 0, 0, TimeSpan.Zero),
            LocationName = "Test Location",
            Street = "Test Street 1",
            City = "Munich",
            State = "Bavaria",
            PostalCode = "80331",
            Country = "Germany",
        };
        await PostAsync("/api/conferences", createDto);

        // Act
        var response = await HttpClient.GetAsync("/api/conferences");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<GetAllConferencesDto>>();
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Test Conference", result![0].Name);
        Assert.Equal("Munich", result[0].City);
    }

    [Fact]
    public async Task GetConferenceById_ReturnsConference_WhenConferenceExists()
    {
        // Arrange
        var token = await GetAuthenticationToken(
            GetUniqueEmail("organizer"),
            "password123",
            UserRole.Organizer
        );
        SetAuthenticationToken(token);

        var createDto = new CreateConferenceDto
        {
            Name = "Specific Conference",
            Start = new DateTimeOffset(2026, 11, 1, 9, 0, 0, TimeSpan.Zero),
            End = new DateTimeOffset(2026, 11, 3, 18, 0, 0, TimeSpan.Zero),
            LocationName = "Hamburg Convention Center",
            Street = "Messeplatz 1",
            City = "Hamburg",
            State = "Hamburg",
            PostalCode = "20357",
            Country = "Germany",
        };
        var createResponse = await PostAsync("/api/conferences", createDto);
        var createdConference = await DeserializeResponse<ConferenceCreatedDto>(createResponse);

        // Act
        var response = await HttpClient.GetAsync($"/api/conferences/{createdConference!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetConferenceByIdDto>();
        Assert.NotNull(result);
        Assert.Equal("Specific Conference", result!.Name);
        Assert.Equal("Hamburg Convention Center", result.LocationName);
        Assert.Equal("Messeplatz 1", result.Street);
        Assert.Equal("Hamburg", result.City);
    }

    [Fact]
    public async Task RenameConference_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var token = await GetAuthenticationToken(
            GetUniqueEmail("organizer"),
            "password123",
            UserRole.Organizer
        );
        SetAuthenticationToken(token);

        var createDto = new CreateConferenceDto
        {
            Name = "Original Name",
            Start = new DateTimeOffset(2026, 12, 1, 9, 0, 0, TimeSpan.Zero),
            End = new DateTimeOffset(2026, 12, 3, 18, 0, 0, TimeSpan.Zero),
            LocationName = "Test Location",
            Street = "Test Street",
            City = "Test City",
            State = "Test State",
            PostalCode = "12345",
            Country = "Test Country",
        };
        var createResponse = await PostAsync("/api/conferences", createDto);
        var createdConference = await DeserializeResponse<ConferenceCreatedDto>(createResponse);

        var renameDto = new RenameConferenceDto { Name = "Updated Name" };

        // Act
        var response = await PutAsync($"/api/conferences/{createdConference!.Id}/name", renameDto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the name was updated
        var getResponse = await HttpClient.GetAsync($"/api/conferences/{createdConference.Id}");
        var updatedConference = await getResponse.Content.ReadFromJsonAsync<GetConferenceByIdDto>();
        Assert.NotNull(updatedConference);
        Assert.Equal("Updated Name", updatedConference!.Name);
    }

    [Fact]
    public async Task RenameConference_WithoutAuthorization_ReturnsUnauthorized()
    {
        // Arrange
        var conferenceId = Guid.NewGuid();
        var renameDto = new RenameConferenceDto { Name = "New Name" };

        // Act
        var response = await PutAsync($"/api/conferences/{conferenceId}/name", renameDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
