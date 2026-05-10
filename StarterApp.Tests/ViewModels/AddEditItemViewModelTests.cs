using StarterApp.Models;
using StarterApp.Repositories;
using StarterApp.Services;
using StarterApp.ViewModels;
using NSubstitute;
using Xunit;

namespace StarterApp.Tests.ViewModels;

// Tests for AddEditItemViewModel — covers the form validation logic
// NSubstitute creates fake (mock) versions of IItemRepository and INavigationService
// so the tests never make real HTTP calls
public class AddEditItemViewModelTests
{
    private readonly IItemRepository _itemRepo = Substitute.For<IItemRepository>();
    private readonly INavigationService _navService = Substitute.For<INavigationService>();

    private AddEditItemViewModel CreateVm() => new(_itemRepo, _navService);

    [Fact]
    public void InitialState_IsAddMode_WithCorrectTitle()
    {
        var vm = CreateVm();
        Assert.False(vm.IsEditMode);
        Assert.Equal("Add Item", vm.Title);
    }

    // When a positive ItemId is set (via navigation), the VM should switch to edit mode
    [Fact]
    public void OnItemIdChanged_WhenPositive_SetsEditMode()
    {
        var vm = CreateVm();
        _itemRepo.GetByIdAsync(Arg.Any<int>()).Returns((Item?)null);
        vm.ItemId = 5;
        Assert.True(vm.IsEditMode);
        Assert.Equal("Edit Item", vm.Title);
    }

    // Validation: title cannot be empty
    [Fact]
    public async Task Save_WithEmptyTitle_SetsError()
    {
        var vm = CreateVm();
        vm.ItemTitle = "";
        await vm.SaveCommand.ExecuteAsync(null);
        Assert.True(vm.HasError);
        Assert.Contains("5 characters", vm.ErrorMessage);
    }

    // Validation: title must be at least 5 characters
    [Fact]
    public async Task Save_WithTitleTooShort_SetsError()
    {
        var vm = CreateVm();
        vm.ItemTitle = "abc";
        await vm.SaveCommand.ExecuteAsync(null);
        Assert.True(vm.HasError);
        Assert.Contains("5 characters", vm.ErrorMessage);
    }

    // Validation: daily rate must be a valid decimal number
    [Fact]
    public async Task Save_WithInvalidRate_SetsError()
    {
        var vm = CreateVm();
        vm.ItemTitle = "Valid Title";
        vm.DailyRateText = "not-a-number";
        await vm.SaveCommand.ExecuteAsync(null);
        Assert.True(vm.HasError);
        Assert.Contains("valid daily rate", vm.ErrorMessage);
    }

    // Validation: daily rate must be £1000 or less
    [Fact]
    public async Task Save_WithRateTooHigh_SetsError()
    {
        var vm = CreateVm();
        vm.ItemTitle = "Valid Title";
        vm.DailyRateText = "1001";
        await vm.SaveCommand.ExecuteAsync(null);
        Assert.True(vm.HasError);
        Assert.Contains("valid daily rate", vm.ErrorMessage);
    }

    // Validation: daily rate must be greater than zero
    [Fact]
    public async Task Save_WithZeroRate_SetsError()
    {
        var vm = CreateVm();
        vm.ItemTitle = "Valid Title";
        vm.DailyRateText = "0";
        await vm.SaveCommand.ExecuteAsync(null);
        Assert.True(vm.HasError);
    }

    // Validation: a category must be selected
    [Fact]
    public async Task Save_WithNoCategory_SetsError()
    {
        var vm = CreateVm();
        vm.ItemTitle = "Valid Title";
        vm.DailyRateText = "10.00";
        vm.SelectedCategory = null;
        await vm.SaveCommand.ExecuteAsync(null);
        Assert.True(vm.HasError);
        Assert.Contains("category", vm.ErrorMessage);
    }

    // Happy path: all valid data should result in a call to CreateAsync on the repository
    [Fact]
    public async Task Save_WithValidData_CallsCreateAsync()
    {
        var vm = CreateVm();
        vm.ItemTitle = "Valid Item Title";
        vm.DailyRateText = "25.00";
        vm.SelectedCategory = new Category { Id = 1, Name = "Electronics" };
        _itemRepo.CreateAsync(Arg.Any<Item>()).Returns(new Item());

        await vm.SaveCommand.ExecuteAsync(null);

        await _itemRepo.Received(1).CreateAsync(Arg.Any<Item>());
        Assert.False(vm.HasError);
    }

    // The ViewModel should trim whitespace from the title before saving
    [Fact]
    public async Task Save_ValidData_TrimsTitle()
    {
        Item? savedItem = null;
        _itemRepo.CreateAsync(Arg.Do<Item>(i => savedItem = i)).Returns(new Item());
        var vm = CreateVm();
        vm.ItemTitle = "  My Item  ";
        vm.DailyRateText = "10.00";
        vm.SelectedCategory = new Category { Id = 1, Name = "Tools" };

        await vm.SaveCommand.ExecuteAsync(null);

        Assert.Equal("My Item", savedItem?.Title);
    }

    // Edit mode: a valid save should call UpdateAsync, not CreateAsync
    [Fact]
    public async Task Save_InEditMode_CallsUpdateAsyncNotCreate()
    {
        _itemRepo.GetByIdAsync(5).Returns((Item?)null);
        _itemRepo.UpdateAsync(Arg.Any<int>(), Arg.Any<Item>()).Returns(new Item());
        var vm = CreateVm();
        vm.ItemId = 5; // switches to edit mode
        vm.ItemTitle = "Updated Title";
        vm.DailyRateText = "15.00";
        vm.SelectedCategory = new Category { Id = 2, Name = "Tools" };

        await vm.SaveCommand.ExecuteAsync(null);

        await _itemRepo.Received(1).UpdateAsync(5, Arg.Any<Item>());
        await _itemRepo.DidNotReceive().CreateAsync(Arg.Any<Item>());
    }

    // Cancel should navigate back without calling the repository at all
    [Fact]
    public async Task Cancel_CallsNavigateBack()
    {
        var vm = CreateVm();
        await vm.CancelCommand.ExecuteAsync(null);
        await _navService.Received(1).NavigateBackAsync();
    }

    // InitializeAsync populates the Categories collection from the repository
    [Fact]
    public async Task InitializeAsync_LoadsCategories()
    {
        _itemRepo.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = 1, Name = "Electronics" },
            new() { Id = 2, Name = "Tools" }
        });
        var vm = CreateVm();

        await vm.InitializeAsync();

        Assert.Equal(2, vm.Categories.Count);
    }

    // InitializeAsync in edit mode pre-fills form fields from the existing item
    [Fact]
    public async Task InitializeAsync_InEditMode_PopulatesFormFields()
    {
        _itemRepo.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = 1, Name = "Electronics" }
        });
        _itemRepo.GetByIdAsync(3).Returns(new Item
        {
            Id = 3,
            Title = "Existing Item",
            DailyRate = 20.00m,
            CategoryId = 1
        });
        var vm = CreateVm();
        vm.ItemId = 3;

        await vm.InitializeAsync();

        Assert.Equal("Existing Item", vm.ItemTitle);
        Assert.Equal("20.00", vm.DailyRateText);
    }
}
