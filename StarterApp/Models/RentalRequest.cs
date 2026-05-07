namespace StarterApp.Models;

public class RentalRequest
{
    public int ItemId { get; set; }
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
}
