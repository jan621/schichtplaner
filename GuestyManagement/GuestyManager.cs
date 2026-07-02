using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CrossCutting.Entities;
using CrossCutting.Entities.NotMapped.Guesty;
using DatabaseManagement.Contract;
using Datastoring.EfCore;
using GuestyManagement.Contract;
using Microsoft.Extensions.Configuration;
using UserManagement.Contract;

namespace GuestyManagement;

public class GuestyManager(
    IConfiguration configuration,
    HttpClient httpClient,
    IDatabaseManager<User, PlanerIdentityContext> databaseManager,
    IUserManager userManager)
    : IGuestyManager
{
    private readonly string _apiUrl = configuration["ApiUrls:Guesty"]
                                      ?? throw new Exception("No Guesty api url configured");

    private readonly string _oauthUrl = configuration["ApiUrls:GuestyOAuth"]
                                        ?? throw new Exception("No Guesty oauth url configured");

    public async Task<bool> SaveCredentialsAsync(string clientId, string clientSecret)
    {
        var token = await RequestTokenAsync(clientId, clientSecret);
        if (token == null)
            return false;

        var currentUser = await userManager.GetCurrentUserAsync();
        currentUser.GuestyClientId = clientId;
        currentUser.GuestyClientSecret = clientSecret;
        currentUser.GuestyAccessToken = token.AccessToken;
        currentUser.GuestyTokenExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn);
        return await databaseManager.UpdateAsync(currentUser, true);
    }

    public async Task<string> GetUserClientIdAsync()
    {
        var currentUser = await userManager.GetCurrentUserAsync();
        return currentUser.GuestyClientId ?? string.Empty;
    }

    public async Task<IEnumerable<BasicAccommodation>?> GetBasicAccommodationsAsync()
    {
        var accommodations = new List<BasicAccommodation>();

        var accessToken = await GetAccessTokenAsync();
        if (string.IsNullOrEmpty(accessToken))
            return accommodations;

        try
        {
            var skip = 0;
            const int limit = 100;
            while (true)
            {
                var url = $"{_apiUrl}/listings?fields=title%20nickname&limit={limit}&skip={skip}";
                var response = await SendAuthorizedAsync(url, accessToken);
                if (response == null || !response.IsSuccessStatusCode)
                    return accommodations;

                var listings = await response.Content.ReadFromJsonAsync<GuestyListResponse<GuestyListing>>();
                if (listings?.Results == null || !listings.Results.Any())
                    break;

                accommodations.AddRange(listings.Results.Select(l => new BasicAccommodation
                {
                    Id = l.Id ?? string.Empty,
                    Name = string.IsNullOrWhiteSpace(l.Nickname) ? l.Title : l.Nickname
                }));

                skip += limit;
                if (accommodations.Count >= listings.Count)
                    break;
            }
        }
        catch (HttpRequestException)
        {
            // Guesty not reachable - the dialogs then simply show no accommodations.
        }

        return accommodations;
    }

    public async Task<RootBooking> GetBookingsAsync()
    {
        var bookings = new RootBooking
        {
            Bookings = new List<Booking>()
        };

        var accessToken = await GetAccessTokenAsync();
        if (string.IsNullOrEmpty(accessToken))
            return bookings;

        // Upcoming, non-canceled reservations (check-out today or later).
        var filters = JsonSerializer.Serialize(new object[]
        {
            new { field = "checkOutDateLocalized", @operator = "$gte", value = DateTime.Today.ToString("yyyy-MM-dd") },
            new { field = "status", @operator = "$in", value = new[] { "confirmed", "checked_in" } }
        });

        const string fields = "checkInDateLocalized%20checkOutDateLocalized%20guestsCount%20listing";

        try
        {
            var skip = 0;
            const int limit = 100;
            while (true)
            {
                var url = $"{_apiUrl}/reservations?fields={fields}&sort=checkInDateLocalized" +
                          $"&filters={Uri.EscapeDataString(filters)}&limit={limit}&skip={skip}";
                var response = await SendAuthorizedAsync(url, accessToken);
                if (response == null || !response.IsSuccessStatusCode)
                    return bookings;

                var reservations = await response.Content.ReadFromJsonAsync<GuestyListResponse<GuestyReservation>>();
                if (reservations?.Results == null || !reservations.Results.Any())
                    break;

                bookings.Bookings.AddRange(reservations.Results.Select(r => new Booking
                {
                    Id = r.Id ?? string.Empty,
                    Arrival = r.CheckInDateLocalized,
                    Departure = r.CheckOutDateLocalized,
                    Adults = r.GuestsCount ?? 0,
                    Children = 0,
                    Apartment = new Apartment
                    {
                        Id = r.Listing?.Id ?? string.Empty,
                        Name = string.IsNullOrWhiteSpace(r.Listing?.Nickname) ? r.Listing?.Title : r.Listing?.Nickname
                    }
                }));

                skip += limit;
                if (bookings.Bookings.Count >= reservations.Count)
                    break;
            }
        }
        catch (HttpRequestException)
        {
            // Guesty not reachable - show the calendar without bookings
            // instead of failing the whole page.
        }

        return bookings;
    }

    private async Task<string> GetAccessTokenAsync()
    {
        var currentUser = await userManager.GetCurrentUserAsync();

        if (string.IsNullOrWhiteSpace(currentUser.GuestyClientId) ||
            string.IsNullOrWhiteSpace(currentUser.GuestyClientSecret))
            return string.Empty;

        // Guesty tokens are valid for 24h and token requests are rate limited,
        // so reuse the stored token as long as it is still valid.
        if (!string.IsNullOrEmpty(currentUser.GuestyAccessToken) &&
            currentUser.GuestyTokenExpiresAt.HasValue &&
            currentUser.GuestyTokenExpiresAt.Value > DateTime.UtcNow.AddMinutes(5))
            return currentUser.GuestyAccessToken;

        var token = await RequestTokenAsync(currentUser.GuestyClientId, currentUser.GuestyClientSecret);
        if (token == null)
            return string.Empty;

        currentUser.GuestyAccessToken = token.AccessToken;
        currentUser.GuestyTokenExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn);
        await databaseManager.UpdateAsync(currentUser, true);

        return token.AccessToken ?? string.Empty;
    }

    private async Task<GuestyTokenResponse?> RequestTokenAsync(string clientId, string clientSecret)
    {
        try
        {
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, _oauthUrl)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["scope"] = "open-api",
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret
                })
            };

            var tokenResponse = await httpClient.SendAsync(tokenRequest);
            if (!tokenResponse.IsSuccessStatusCode)
                return null;

            var token = await tokenResponse.Content.ReadFromJsonAsync<GuestyTokenResponse>();
            return string.IsNullOrEmpty(token?.AccessToken) ? null : token;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private async Task<HttpResponseMessage?> SendAuthorizedAsync(string url, string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        request.Headers.Add("Accept", "application/json");
        return await httpClient.SendAsync(request);
    }

    private class GuestyTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }

    private class GuestyListResponse<T>
    {
        [JsonPropertyName("results")]
        public List<T>? Results { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    private class GuestyListing
    {
        [JsonPropertyName("_id")]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("nickname")]
        public string? Nickname { get; set; }
    }

    private class GuestyReservation
    {
        [JsonPropertyName("_id")]
        public string? Id { get; set; }

        [JsonPropertyName("checkInDateLocalized")]
        public string? CheckInDateLocalized { get; set; }

        [JsonPropertyName("checkOutDateLocalized")]
        public string? CheckOutDateLocalized { get; set; }

        [JsonPropertyName("guestsCount")]
        public int? GuestsCount { get; set; }

        [JsonPropertyName("listing")]
        public GuestyListing? Listing { get; set; }
    }
}
