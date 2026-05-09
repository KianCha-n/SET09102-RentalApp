using System.Net.Http.Json;
using StarterApp.Models;

namespace StarterApp.Repositories;

// Handles all API calls for reviews using the shared HttpClient
public class ReviewRepository : IReviewRepository
{
    private readonly HttpClient _httpClient;

    public ReviewRepository(HttpClient httpClient) => _httpClient = httpClient;

    // Calls GET /items/{id}/reviews and unpacks the paginated response wrapper
    public async Task<IEnumerable<Review>> GetItemReviewsAsync(int itemId)
    {
        var response = await _httpClient.GetFromJsonAsync<ReviewsResponse>($"items/{itemId}/reviews");
        return response?.Reviews ?? new List<Review>();
    }

    // Calls POST /reviews with the rental ID, rating, and optional comment
    public async Task<Review> CreateAsync(ReviewRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("reviews", new
        {
            rentalId = request.RentalId,
            rating = request.Rating,
            comment = request.Comment
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Review>()
            ?? throw new InvalidOperationException("Server returned empty response");
    }

    // Private record that matches the API's JSON response shape for the reviews list
    private record ReviewsResponse(List<Review> Reviews, double AverageRating, int TotalReviews,
        int Page, int PageSize, int TotalPages);
}
