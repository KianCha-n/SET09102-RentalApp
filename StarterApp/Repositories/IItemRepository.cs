using StarterApp.Models;

namespace StarterApp.Repositories;

public interface IItemRepository : IRepository<Item>
{
    Task<IEnumerable<Category>> GetCategoriesAsync();
}
