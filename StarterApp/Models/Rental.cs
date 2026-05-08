namespace StarterApp.Models;

public class Rental
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string ItemTitle { get; set; } = string.Empty;
    public int BorrowerId { get; set; }
    public string BorrowerName { get; set; } = string.Empty;
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }

    public string FormattedPrice => $"£{TotalPrice:F2}";

    private static string FormatDate(string raw) =>
        DateTime.TryParse(raw, out var d) ? d.ToString("dd MMM yyyy") : raw;

    public string DateRange => $"{FormatDate(StartDate)} – {FormatDate(EndDate)}";
    public string StatusDisplay => Status.ToLowerInvariant() switch
    {
        "pending" => "Pending",
        "approved" => "Approved",
        "rejected" => "Rejected",
        "completed" => "Completed",
        "cancelled" => "Cancelled",
        _ => Status
    };
}
