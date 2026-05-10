using StarterApp.Database.Models;
using StarterApp.Models;
using StarterApp.Repositories;
using StarterApp.Services;
using StarterApp.ViewModels;
using NSubstitute;
using Xunit;

namespace StarterApp.Tests.ViewModels;

// Tests for ItemDetailViewModel — covers ownership detection, token expiry, and navigation guards
public class ItemDetailViewModelTests
{
    private readonly IItemRepository _itemRepo = Substitute.For<IItemRepository>();
    private readonly INavigationService _navService = Substitute.For<INavigationService>();
    private readonly IAuthenticationService _authService = Substitute.For<IAuthenticationService>();

    private ItemDetailViewModel CreateVm() => new(_itemRepo, _navService, _authService);

    [Fact]
    public void InitialState_HasCorrectTitleNullItemAndNotOwner()
    {
        var vm = CreateVm();
        Assert.Equal("Item Details", vm.Title);
        Assert.Null(vm.Item);
        Assert.False(vm.IsOwner);
        Assert.False(vm.IsBusy);
    }

    // Guard: ItemId == 0 (default) must exit before touching the repository
    [Fact]
    public async Task LoadItem_WhenItemIdIsZero_DoesNotCallRepository()
    {
        var vm = CreateVm();
        await vm.LoadItemCommand.ExecuteAsync(null);
        await _itemRepo.DidNotReceive().GetByIdAsync(Arg.Any<int>());
    }

    // Security: an expired token must redirect to login without loading any item data
    [Fact]
    public async Task LoadItem_WhenTokenExpired_LogsOutAndRedirects()
    {
        _authService.IsTokenExpired.Returns(true);
        var vm = CreateVm();
        vm.ItemId = 1;

        await vm.LoadItemCommand.ExecuteAsync(null);

        // Received() checks at-least-once — fire-and-forget from OnItemIdChanged may add extra calls
        await _authService.Received().LogoutAsync();
        await _navService.Received().NavigateToAsync("//LoginPage");
        Assert.Null(vm.Item);
    }

    // Happy path: a found item populates Item, updates the page title, and clears IsBusy
    [Fact]
    public async Task LoadItem_WhenItemFound_SetsItemTitleAndNotOwner()
    {
        _authService.IsTokenExpired.Returns(false);
        _authService.CurrentUser.Returns(new User { Id = 10 });
        _itemRepo.GetByIdAsync(3).Returns(new Item { Id = 3, Title = "Electric Drill", OwnerId = 99 });
        var vm = CreateVm();
        vm.ItemId = 3;

        await vm.LoadItemCommand.ExecuteAsync(null);

        Assert.NotNull(vm.Item);
        Assert.Equal("Electric Drill", vm.Title);
        Assert.False(vm.IsOwner); // user 10 != owner 99
        Assert.False(vm.IsBusy);
    }

    // IsOwner must be true when the logged-in user's ID matches the item's OwnerId
    [Fact]
    public async Task LoadItem_WhenCurrentUserOwnsItem_SetsIsOwnerTrue()
    {
        _authService.IsTokenExpired.Returns(false);
        _authService.CurrentUser.Returns(new User { Id = 42 });
        _itemRepo.GetByIdAsync(5).Returns(new Item { Id = 5, Title = "Ladder", OwnerId = 42 });
        var vm = CreateVm();
        vm.ItemId = 5;

        await vm.LoadItemCommand.ExecuteAsync(null);

        Assert.True(vm.IsOwner);
    }

    // A repository exception must set HasError and stop the busy indicator
    [Fact]
    public async Task LoadItem_WhenRepositoryThrows_SetsErrorAndStopsBusy()
    {
        _authService.IsTokenExpired.Returns(false);
        _itemRepo.GetByIdAsync(Arg.Any<int>())
            .Returns(Task.FromException<Item?>(new Exception("Server error")));
        var vm = CreateVm();
        vm.ItemId = 1;

        await vm.LoadItemCommand.ExecuteAsync(null);

        Assert.True(vm.HasError);
        Assert.Contains("Failed to load item", vm.ErrorMessage);
        Assert.False(vm.IsBusy);
    }

    // RentItem with no loaded item (null guard) should not trigger navigation
    [Fact]
    public async Task RentItem_WhenItemIsNull_DoesNotNavigate()
    {
        var vm = CreateVm();
        await vm.RentItemCommand.ExecuteAsync(null);
        await _navService.DidNotReceive().NavigateToAsync(Arg.Any<string>(), Arg.Any<Dictionary<string, object>>());
    }

    // RentItem with a valid item navigates to the rental request page with the correct ItemId
    [Fact]
    public async Task RentItem_WhenItemIsLoaded_NavigatesToRentalRequestPage()
    {
        _authService.IsTokenExpired.Returns(false);
        _authService.CurrentUser.Returns(new User { Id = 1 });
        _itemRepo.GetByIdAsync(7).Returns(new Item { Id = 7, Title = "Saw" });
        var vm = CreateVm();
        vm.ItemId = 7;
        await vm.LoadItemCommand.ExecuteAsync(null);

        await vm.RentItemCommand.ExecuteAsync(null);

        await _navService.Received().NavigateToAsync("RentalRequestPage",
            Arg.Is<Dictionary<string, object>>(d => (int)d["ItemId"] == 7));
    }

    // EditItem with no loaded item (null guard) should not trigger navigation
    [Fact]
    public async Task EditItem_WhenItemIsNull_DoesNotNavigate()
    {
        var vm = CreateVm();
        await vm.EditItemCommand.ExecuteAsync(null);
        await _navService.DidNotReceive().NavigateToAsync(Arg.Any<string>(), Arg.Any<Dictionary<string, object>>());
    }

    // EditItem with a valid item navigates to AddEditItemPage with the correct ItemId
    [Fact]
    public async Task EditItem_WhenItemIsLoaded_NavigatesToAddEditItemPage()
    {
        _authService.IsTokenExpired.Returns(false);
        _itemRepo.GetByIdAsync(7).Returns(new Item { Id = 7, Title = "Saw" });
        var vm = CreateVm();
        vm.ItemId = 7;
        await vm.LoadItemCommand.ExecuteAsync(null);

        await vm.EditItemCommand.ExecuteAsync(null);

        await _navService.Received().NavigateToAsync("AddEditItemPage",
            Arg.Is<Dictionary<string, object>>(d => (int)d["ItemId"] == 7));
    }

    // GoBack should always delegate to the navigation service regardless of item state
    [Fact]
    public async Task GoBack_CallsNavigateBack()
    {
        var vm = CreateVm();
        await vm.GoBackCommand.ExecuteAsync(null);
        await _navService.Received(1).NavigateBackAsync();
    }
}
