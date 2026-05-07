using System.Net.Http.Json;
using StarterApp.Models;

namespace StarterApp.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly HttpClient _httpClient;

    public ItemRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Item>> GetAllAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<PagedItemsResponse>("items");
        return response?.Items ?? new List<Item>();
    }

    public async Task<Item?> GetByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<Item>($"items/{id}");
    }

    public async Task<Item> CreateAsync(Item item)
    {
        var response = await _httpClient.PostAsJsonAsync("items", new
        {
            title = item.Title,
            description = item.Description,
            dailyRate = item.DailyRate,
            categoryId = item.CategoryId,
            latitude = item.Latitude ?? 55.9533,
            longitude = item.Longitude ?? -3.1883
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Item>() ?? item;
    }

    public async Task<Item> UpdateAsync(int id, Item item)
    {
        var response = await _httpClient.PutAsJsonAsync($"items/{id}", new
        {
            title = item.Title,
            description = item.Description,
            dailyRate = item.DailyRate,
            isAvailable = item.IsAvailable
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Item>() ?? item;
    }

    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<CategoriesResponse>("categories");
        return response?.Categories ?? new List<Category>();
    }

    private record PagedItemsResponse(List<Item> Items, int TotalItems, int Page, int PageSize, int TotalPages);
    private record CategoriesResponse(List<Category> Categories);
}
