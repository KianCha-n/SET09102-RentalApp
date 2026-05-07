using StarterApp.Models;

namespace StarterApp.Repositories;

public interface IRentalRepository
{
    Task<IEnumerable<Rental>> GetIncomingAsync();
    Task<IEnumerable<Rental>> GetOutgoingAsync();
    Task<Rental?> GetByIdAsync(int id);
    Task<Rental> CreateAsync(RentalRequest request);
    Task UpdateStatusAsync(int id, string status);
}
