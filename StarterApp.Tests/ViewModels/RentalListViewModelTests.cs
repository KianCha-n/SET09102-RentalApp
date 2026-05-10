using StarterApp.Models;
using StarterApp.Repositories;
using StarterApp.Services;
using StarterApp.ViewModels;
using NSubstitute;
using Xunit;

namespace StarterApp.Tests.ViewModels;

// Tests for RentalListViewModel — covers tab switching, token expiry, and rental loading
public class RentalListViewModelTests
{
    private readonly IRentalRepository _rentalRepo = Substitute.For<IRentalRepository>();
    private readonly INavigationService _navService = Substitute.For<INavigationService>();
    private readonly IAuthenticationService _authService = Substitute.For<IAuthenticationService>();

    private RentalListViewModel CreateVm() => new(_rentalRepo, _navService, _authService);

    [Fact]
    public void InitialState_ShowsIncomingTabAndCorrectTitle()
    {
        var vm = CreateVm();
        Assert.True(vm.ShowIncoming);   // incoming tab is shown by default
        Assert.Equal("My Rentals", vm.Title);
        Assert.Empty(vm.IncomingRentals);
        Assert.Empty(vm.OutgoingRentals);
    }

    // Tab switch: tapping "Lent Out" then "Borrowed" should show incoming
    [Fact]
    public void ShowIncomingTab_SetsShowIncomingTrue()
    {
        var vm = CreateVm();
        vm.ShowOutgoingTabCommand.Execute(null);
        vm.ShowIncomingTabCommand.Execute(null);
        Assert.True(vm.ShowIncoming);
    }

    // Tab switch: tapping "Lent Out" should show outgoing
    [Fact]
    public void ShowOutgoingTab_SetsShowIncomingFalse()
    {
        var vm = CreateVm();
        vm.ShowOutgoingTabCommand.Execute(null);
        Assert.False(vm.ShowIncoming);
    }

    // Security test: expired token must log out and navigate, with no API calls made
    [Fact]
    public async Task LoadRentals_WhenTokenExpired_LogsOutAndNavigatesToLogin()
    {
        _authService.IsTokenExpired.Returns(true);
        var vm = CreateVm();

        await vm.LoadRentalsCommand.ExecuteAsync(null);

        await _authService.Received(1).LogoutAsync();
        await _navService.Received(1).NavigateToAsync("//LoginPage");
        await _rentalRepo.DidNotReceive().GetIncomingAsync(); // no API call
    }

    // Happy path: rentals from the repository should populate both collections
    [Fact]
    public async Task LoadRentals_WhenTokenValid_PopulatesBothCollections()
    {
        _authService.IsTokenExpired.Returns(false);
        _rentalRepo.GetIncomingAsync().Returns(new List<Rental>
        {
            new() { Id = 1, ItemTitle = "Drill" }
        });
        _rentalRepo.GetOutgoingAsync().Returns(new List<Rental>
        {
            new() { Id = 2, ItemTitle = "Ladder" },
            new() { Id = 3, ItemTitle = "Saw" }
        });
        var vm = CreateVm();

        await vm.LoadRentalsCommand.ExecuteAsync(null);

        Assert.Single(vm.IncomingRentals);
        Assert.Equal(2, vm.OutgoingRentals.Count);
        Assert.False(vm.HasError);
        Assert.False(vm.IsRefreshing); // pull-to-refresh spinner should stop
    }

    // Error handling: a failed API call should show an error and stop the spinner
    [Fact]
    public async Task LoadRentals_WhenRepositoryThrows_SetsErrorAndStopsRefreshing()
    {
        _authService.IsTokenExpired.Returns(false);
        _rentalRepo.GetIncomingAsync()
            .Returns(Task.FromException<IEnumerable<Rental>>(new Exception("API error")));
        var vm = CreateVm();

        await vm.LoadRentalsCommand.ExecuteAsync(null);

        Assert.True(vm.HasError);
        Assert.Contains("Failed to load rentals", vm.ErrorMessage);
        Assert.False(vm.IsRefreshing);
    }
}
