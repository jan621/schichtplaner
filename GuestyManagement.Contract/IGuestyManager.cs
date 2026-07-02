using CrossCutting.Entities.NotMapped.Guesty;

namespace GuestyManagement.Contract;

public interface IGuestyManager
{
    /// <summary>
    /// Validates the given credentials against the Guesty OAuth endpoint and,
    /// when valid, stores them (including the fetched access token) on the
    /// current user. Returns false when the credentials are rejected.
    /// </summary>
    Task<bool> SaveCredentialsAsync(string clientId, string clientSecret);

    Task<string> GetUserClientIdAsync();

    Task<IEnumerable<BasicAccommodation>?> GetBasicAccommodationsAsync();

    Task<RootBooking> GetBookingsAsync();
}
