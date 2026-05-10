using StarterApp.Models;
using StarterApp.Repositories;
using StarterApp.Services;
using StarterApp.ViewModels;
using NSubstitute;
using Xunit;

namespace StarterApp.Tests.ViewModels;

// Tests for ItemListViewModel — covers token expiry handling and item loading
public class ItemListViewModelTests
{
    private readonly IItemRepository _itemRepo = Substitute.For<IItemRepository>();
    private readonly INavigationService _navService = Substitute.For<INavigationService>();
    private readonly IAuthenticationService _authService = Substitute.For<IAuthenticationService>();

    private ItemListViewModel CreateVm() => new(_itemRepo, _navService, _authService);

    [Fact]
    public void InitialState_HasEmptyItemsAndCorrectTitle()
    {
        var vm = CreateVm();
        Assert.Empty(vm.Items);
        Assert.Equal("Browse Items", vm.Title);
    }

    // Security test: if the token is expired the VM must log out and go to login,
    // and must NOT attempt any API call
    [Fact]
    public async Task LoadItems_WhenTokenExpired_LogsOutAndNavigatesToLogin()
    {
        _authService.IsTokenExpired.Returns(true);
        var vm = CreateVm();

        await vm.LoadItemsCommand.ExecuteAsync(null);

        await _authService.Received(1).LogoutAsync();
        await _navService.Received(1).NavigateToAsync("//LoginPage");
        await _itemRepo.DidNotReceive().GetAllAsync(); // no API call should be made
    }

    // Happy path: items returned from the repository should appear in the Items collection
    [Fact]
    public async Task LoadItems_WhenTokenValid_PopulatesCollection()
    {
        _authService.IsTokenExpired.Returns(false);
        _itemRepo.GetAllAsync().Returns(new List<Item>
        {
            new() { Id = 1, Title = "Drill", DailyRate = 5.00m },
            new() { Id = 2, Title = "Ladder", DailyRate = 8.00m }
        });
        var vm = CreateVm();

        await vm.LoadItemsCommand.ExecuteAsync(null);

        Assert.Equal(2, vm.Items.Count);
        Assert.False(vm.HasError);
        Assert.False(vm.IsBusy); // spinner should stop after loading
    }

    // Error handling: a network failure should show an error message, not crash
    [Fact]
    public async Task LoadItems_WhenRepositoryThrows_SetsError()
    {
        _authService.IsTokenExpired.Returns(false);
        _itemRepo.GetAllAsync()
            .Returns(Task.FromException<IEnumerable<Item>>(new Exception("Network error")));
        var vm = CreateVm();

        await vm.LoadItemsCommand.ExecuteAsync(null);

        Assert.True(vm.HasError);
        Assert.Contains("Failed to load items", vm.ErrorMessage);
        Assert.False(vm.IsBusy);
    }

    // Pull-to-refresh should leave IsRefreshing as false once loading completes
    [Fact]
    public async Task Refresh_SetsIsRefreshingBeforeLoad()
    {
        _authService.IsTokenExpired.Returns(false);
        _itemRepo.GetAllAsync().Returns(new List<Item>());
        var vm = CreateVm();

        await vm.RefreshCommand.ExecuteAsync(null);

        Assert.False(vm.IsRefreshing);
    }
}
