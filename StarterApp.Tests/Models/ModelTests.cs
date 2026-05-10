using StarterApp.Models;
using Xunit;

namespace StarterApp.Tests.Models;

// Tests for Item model computed properties
public class ItemModelTests
{
    // FormattedRate should always show £ sign with two decimal places
    [Fact]
    public void FormattedRate_ReturnsCorrectFormat()
    {
        var item = new Item { DailyRate = 10.5m };
        Assert.Equal("£10.50/day", item.FormattedRate);
    }

    [Fact]
    public void AvailabilityText_WhenAvailable_ReturnsAvailable()
    {
        var item = new Item { IsAvailable = true };
        Assert.Equal("Available", item.AvailabilityText);
    }

    [Fact]
    public void AvailabilityText_WhenUnavailable_ReturnsUnavailable()
    {
        var item = new Item { IsAvailable = false };
        Assert.Equal("Unavailable", item.AvailabilityText);
    }
}

// Tests for Rental model computed properties
public class RentalModelTests
{
    // FormattedPrice should show £ sign with two decimal places
    [Fact]
    public void FormattedPrice_ReturnsCorrectFormat()
    {
        var rental = new Rental { TotalPrice = 25.5m };
        Assert.Equal("£25.50", rental.FormattedPrice);
    }

    // StatusDisplay maps lowercase API status strings to readable display values
    [Theory]
    [InlineData("pending", "Pending")]
    [InlineData("approved", "Approved")]
    [InlineData("rejected", "Rejected")]
    [InlineData("completed", "Completed")]
    [InlineData("cancelled", "Cancelled")]
    [InlineData("PENDING", "Pending")]   // case-insensitive input
    [InlineData("unknown", "unknown")]   // unrecognised statuses pass through unchanged
    public void StatusDisplay_ConvertsCorrectly(string input, string expected)
    {
        var rental = new Rental { Status = input };
        Assert.Equal(expected, rental.StatusDisplay);
    }

    // DateRange should parse ISO dates and format them as "15 Jan 2025 – 20 Jan 2025"
    [Fact]
    public void DateRange_ContainsFormattedDates()
    {
        var rental = new Rental { StartDate = "2025-01-15", EndDate = "2025-01-20" };
        Assert.Contains("Jan", rental.DateRange);
        Assert.Contains("2025", rental.DateRange);
        Assert.Contains("–", rental.DateRange);
    }

    // If the API returns an unparseable date, it should fall back to the raw string
    [Fact]
    public void DateRange_WithInvalidDate_FallsBackToRawString()
    {
        var rental = new Rental { StartDate = "bad-date", EndDate = "also-bad" };
        Assert.Contains("bad-date", rental.DateRange);
    }
}

// Tests for Review model computed properties
public class ReviewModelTests
{
    [Fact]
    public void FormattedRating_ReturnsSlashFiveFormat()
    {
        var review = new Review { Rating = 4 };
        Assert.Equal("4/5", review.FormattedRating);
    }

    // Stars property should fill the right number of ★ and leave ☆ for the rest
    [Fact]
    public void Stars_ContainsCorrectNumberOfFilledStars()
    {
        var review = new Review { Rating = 3 };
        Assert.Equal("★★★☆☆", review.Stars);
    }
}
