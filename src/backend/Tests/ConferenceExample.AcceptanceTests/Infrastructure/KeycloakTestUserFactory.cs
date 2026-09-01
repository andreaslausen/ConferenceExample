using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ConferenceExample.Authentication;

namespace ConferenceExample.AcceptanceTests.Infrastructure;

/// <summary>
/// Creates throwaway Keycloak users with a given app role and mints an access token for them
/// via the Keycloak Admin REST API plus a direct-access-grant test client — the real frontend
/// client only supports the redirect+PKCE flow, which these plain-HTTP, browser-less
/// acceptance tests can't drive.
/// </summary>
public class KeycloakTestUserFactory(string keycloakBaseAddress)
{
    private const string Realm = "conference-example-test";
    private const string TestClientId = "conference-example-tests";
    private const string Password = "Passw0rd!1";

    private static readonly HttpClient HttpClient = new();
    private static readonly JsonSerializerOptions ResponseJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<string> CreateUserAndGetToken(UserRole role)
    {
        var email = $"{Guid.CreateVersion7():N}@test.com";

        var adminToken = await GetAdminToken();
        var userId = await CreateUser(adminToken, email);
        await AssignRealmRole(adminToken, userId, role);
        return await GetUserToken(email);
    }

    private async Task<string> GetAdminToken()
    {
        var response = await HttpClient.PostAsync(
            $"{keycloakBaseAddress}realms/master/protocol/openid-connect/token",
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["client_id"] = "admin-cli",
                    ["username"] = "admin",
                    ["password"] = "admin",
                }
            )
        );
        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(ResponseJsonOptions);
        return token!.AccessToken;
    }

    private async Task<string> CreateUser(string adminToken, string email)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{keycloakBaseAddress}admin/realms/{Realm}/users"
        )
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", adminToken) },
            Content = JsonContent.Create(
                new
                {
                    username = email,
                    email,
                    firstName = "Test",
                    lastName = "User",
                    enabled = true,
                    emailVerified = true,
                    credentials = new[]
                    {
                        new
                        {
                            type = "password",
                            value = Password,
                            temporary = false,
                        },
                    },
                }
            ),
        };

        var response = await HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var location =
            response.Headers.Location
            ?? throw new InvalidOperationException(
                "Keycloak did not return a Location header for the created user."
            );
        return location.Segments[^1];
    }

    private async Task AssignRealmRole(string adminToken, string userId, UserRole role)
    {
        using var getRoleRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"{keycloakBaseAddress}admin/realms/{Realm}/roles/{role}"
        )
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", adminToken) },
        };
        var roleResponse = await HttpClient.SendAsync(getRoleRequest);
        roleResponse.EnsureSuccessStatusCode();
        var roleJson = await roleResponse.Content.ReadAsStringAsync();

        using var assignRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{keycloakBaseAddress}admin/realms/{Realm}/users/{userId}/role-mappings/realm"
        )
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", adminToken) },
            Content = new StringContent($"[{roleJson}]", Encoding.UTF8, "application/json"),
        };
        var assignResponse = await HttpClient.SendAsync(assignRequest);
        assignResponse.EnsureSuccessStatusCode();
    }

    private async Task<string> GetUserToken(string email)
    {
        var response = await HttpClient.PostAsync(
            $"{keycloakBaseAddress}realms/{Realm}/protocol/openid-connect/token",
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["client_id"] = TestClientId,
                    ["username"] = email,
                    ["password"] = Password,
                }
            )
        );
        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(ResponseJsonOptions);
        return token!.AccessToken;
    }

    private record TokenResponse([property: JsonPropertyName("access_token")] string AccessToken);
}
