using StarterApp.Models;

namespace StarterApp.Repositories;

// Defines the contract for all review-related API calls
// Follows the same Repository Pattern used by IItemRepository and IRentalRepository
public interface IReviewRepository
{
    // GET /items/{id}/reviews — fetch all reviews for a specific item
    Task<IEnumerable<Review>> GetItemReviewsAsync(int itemId);

    // POST /reviews — submit a new review after a rental is completed
    Task<Review> CreateAsync(ReviewRequest request);
}
