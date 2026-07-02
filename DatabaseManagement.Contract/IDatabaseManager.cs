using Microsoft.EntityFrameworkCore;

namespace DatabaseManagement.Contract;

public interface IDatabaseManager<TEntity, TContext>
    where TEntity : class
    where TContext : DbContext
{
    Task<bool> AddAsync(TEntity entityToAdd);

    Task<bool> DeleteAsync(TEntity objectToDelete, bool ignoreId = false);

    Task<IEnumerable<TEntity>> GetAllAsync(bool ignoreId = false);

    Task<TEntity?> GetByIdAsync(string id, bool ignoreId = false);

    Task<bool> UpdateAsync(TEntity objectToUpdate, bool ignoreId = false);
}