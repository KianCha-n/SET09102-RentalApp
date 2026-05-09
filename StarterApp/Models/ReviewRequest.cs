namespace StarterApp.Models;

// The data sent to the API when submitting a new review (POST /reviews)
public class ReviewRequest
{
    public int RentalId { get; set; }   // the rental being reviewed
    public int Rating { get; set; }     // 1–5
    public string? Comment { get; set; }
}
