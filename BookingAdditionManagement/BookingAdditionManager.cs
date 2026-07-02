using BookingAdditionManagement.Contract;
using CrossCutting.Entities;
using DatabaseManagement.Contract;
using Datastoring.EfCore;

namespace BookingAdditionManagement;

public class BookingAdditionManager(
    IDatabaseManager<BookingAddition, PlanerIdentityContext> databaseManager) : IBookingAdditionManager
{
    public async Task<IEnumerable<BookingAddition>> GetAllAsync()
    {
        var bookingAdditions = await databaseManager.GetAllAsync();
        return bookingAdditions;
    }

    public async Task<BookingAddition?> GetByIdAsync(string id)
    {
        var bookingAddition = await databaseManager.GetByIdAsync(id);
        return bookingAddition;
    }

    public async Task<bool> CreateAsync(BookingAddition entity)
    {
        var created = await databaseManager.AddAsync(entity);
        return created;
    }

    public async Task<bool> UpdateAsync(BookingAddition entity, bool ignoreId = false)
    {
        var updated = await databaseManager.UpdateAsync(entity, ignoreId);
        return updated;
    }

    public async Task<bool> DeleteAsync(BookingAddition entity, bool ignoreId)
    {
        var deleted = await databaseManager.DeleteAsync(entity, ignoreId);
        return deleted;
    }
}