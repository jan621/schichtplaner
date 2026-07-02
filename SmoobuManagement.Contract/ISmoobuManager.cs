using CrossCutting.Entities.NotMapped.Smoobu;

namespace SmoobuManagement.Contract;

public interface ISmoobuManager
{
    Task<bool> IsApiKeyValidAsync(string apiKey);

    Task<bool> SaveApiKeyAsync(string apiKey);

    Task<string> GetUserApiKeyAsync();

    Task<IEnumerable<BasicAccommodation>?> GetBasicAccommodationsAsync();

    Task<Accommodation> GetAccommodationByIdAsync(int id);

    Task<RootBooking> GetBookingsAsync();
}