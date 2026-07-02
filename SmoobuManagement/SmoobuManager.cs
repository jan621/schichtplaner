using System.Net;
using System.Net.Http.Json;
using System.Web;
using CrossCutting.Entities;
using CrossCutting.Entities.NotMapped.Smoobu;
using DatabaseManagement.Contract;
using Datastoring.EfCore;
using Microsoft.Extensions.Configuration;
using SmoobuManagement.Contract;
using UserManagement.Contract;

namespace SmoobuManagement;

public class SmoobuManager(
    IConfiguration configuration,
    HttpClient httpClient,
    IDatabaseManager<User, PlanerIdentityContext> databaseManager,
    IUserManager userManager)
    : ISmoobuManager
{
    private readonly string _apiUrl = configuration["ApiUrls:Smoobu"]
                                      ?? throw new Exception("No api key found");

    public async Task<bool> IsApiKeyValidAsync(string apiKey)
    {
        var getUserRequest = new HttpRequestMessage()
        {
            RequestUri = new Uri(_apiUrl + "/me"),
            Method = HttpMethod.Get,
            Headers =
            {
                { "Api-Key", apiKey }
            },
        };

        var getUserResponse = await httpClient.SendAsync(getUserRequest);
        return getUserResponse.StatusCode != HttpStatusCode.Unauthorized;
    }

    public async Task<bool> SaveApiKeyAsync(string apiKey)
    {
        var currentUser = await userManager.GetCurrentUserAsync();
        currentUser.SmoobuApiKey = apiKey;
        var saved = await databaseManager.UpdateAsync(currentUser, true);
        return saved;
    }

    public async Task<string> GetUserApiKeyAsync()
    {
        var currentUser = await userManager.GetCurrentUserAsync();
        return currentUser.SmoobuApiKey ?? string.Empty;
    }

    public async Task<IEnumerable<BasicAccommodation>?> GetBasicAccommodationsAsync()
    {
        var getAccommodationsRequest = new HttpRequestMessage()
        {
            RequestUri = new Uri(_apiUrl + "/apartments"),
            Method = HttpMethod.Get,
            Headers =
            {
                { "Api-Key", await GetUserApiKeyAsync() }
            },
        };

        var getAccommodationsResponse = await httpClient.SendAsync(getAccommodationsRequest);

        if (!getAccommodationsResponse.IsSuccessStatusCode)
            return new List<BasicAccommodation>();

        var basicAccommodations = await getAccommodationsResponse.Content.ReadFromJsonAsync<AccommodationResponse>();
        return basicAccommodations?.Apartments;
    }

    public async Task<Accommodation> GetAccommodationByIdAsync(int id)
    {
        var getAccommodationRequest = new HttpRequestMessage()
        {
            RequestUri = new Uri(_apiUrl + "/apartments/" + id),
            Method = HttpMethod.Get,
            Headers =
            {
                { "Api-Key", await GetUserApiKeyAsync() }
            },
        };

        var getAccommodationsResponse = await httpClient.SendAsync(getAccommodationRequest);

        if (!getAccommodationsResponse.IsSuccessStatusCode)
            return new Accommodation();

        var accommodation = await getAccommodationsResponse.Content.ReadFromJsonAsync<Accommodation>();
        return accommodation ?? new Accommodation();
    }

    public async Task<RootBooking> GetBookingsAsync()
    {
        var page_count = 1;
        var uriBuilder = new UriBuilder(_apiUrl + "/reservations");
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        var pageSize = configuration["Settings:Smoobu:PageSize"];
        query["pageSize"] = pageSize;
        var bookings = new RootBooking()
        {
            Bookings = new List<Booking>()
        };

        for (var page = 1; page <= page_count; page++)
        {
            var getBookingsRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                Headers =
                {
                    { "Api-Key", await GetUserApiKeyAsync() }
                },
            };

            query["page"] = page.ToString();
            uriBuilder.Query = query.ToString();
            getBookingsRequest.RequestUri = uriBuilder.Uri;

            var getBookingsResponse = await httpClient.SendAsync(getBookingsRequest);

            if (!getBookingsResponse.IsSuccessStatusCode)
                return new RootBooking();

            var currentBookings = await getBookingsResponse.Content.ReadFromJsonAsync<RootBooking>();
            if (currentBookings == null || currentBookings.Bookings == null ||
                !currentBookings.Bookings.Any())
                break;
            
            page_count = (int)currentBookings.page_count!;

            bookings.Bookings.AddRange(currentBookings.Bookings);
        }

        bookings.Bookings.RemoveAll(b => b.IsBlockedBooking);
        return bookings;
    }
}