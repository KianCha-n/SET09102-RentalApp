namespace StarterApp.Models;

// Represents a review left by a borrower after completing a rental
public class Review
{
    public int Id { get; set; }
    public int RentalId { get; set; }   // the rental this review belongs to
    public int ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public int Rating { get; set; }     // 1–5 stars
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }

    // Computed display helpers used in the UI
    public string FormattedRating => $"{Rating}/5";
    public string Stars => new string('★', Rating) + new string('☆', 5 - Rating);
}
