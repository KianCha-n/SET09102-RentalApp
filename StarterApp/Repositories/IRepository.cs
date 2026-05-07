namespace StarterApp.Repositories;

public interface IRepository<T>
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(int id, T entity);
}
