using System.Net.Http.Json;
using StarterApp.Models;

namespace StarterApp.Repositories;

public class RentalRepository : IRentalRepository
{
    private readonly HttpClient _httpClient;

    public RentalRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Rental>> GetIncomingAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<RentalsResponse>("rentals/incoming");
        return response?.Rentals ?? new List<Rental>();
    }

    public async Task<IEnumerable<Rental>> GetOutgoingAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<RentalsResponse>("rentals/outgoing");
        return response?.Rentals ?? new List<Rental>();
    }

    public async Task<Rental?> GetByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<Rental>($"rentals/{id}");
    }

    public async Task<Rental> CreateAsync(RentalRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("rentals", new
        {
            itemId = request.ItemId,
            startDate = request.StartDate,
            endDate = request.EndDate
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Rental>() ?? new Rental();
    }

    public async Task UpdateStatusAsync(int id, string status)
    {
        var response = await _httpClient.PatchAsJsonAsync($"rentals/{id}/status", new { status });
        response.EnsureSuccessStatusCode();
    }

    private record RentalsResponse(List<Rental> Rentals, int TotalRentals);
}
