namespace StarterApp.Models;

public class Item
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DailyRate { get; set; }
    public int CategoryId { get; set; }
    public string Category { get; set; } = string.Empty;
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsAvailable { get; set; } = true;
    public double? AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public DateTime CreatedAt { get; set; }

    public string FormattedRate => $"£{DailyRate:F2}/day";
    public string AvailabilityText => IsAvailable ? "Available" : "Unavailable";
}
