namespace CrossCutting.Managers;

public interface IEntityManager<T>
{
    Task<IEnumerable<T>> GetAllAsync();

    Task<T?> GetByIdAsync(string id);

    Task<bool> CreateAsync(T entity);

    Task<bool> UpdateAsync(T entity, bool ignoreId = false);

    Task<bool> DeleteAsync(T entity, bool ignoreId = false);
}