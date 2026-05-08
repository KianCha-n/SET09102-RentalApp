using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Models;
using StarterApp.Repositories;
using StarterApp.Services;

namespace StarterApp.ViewModels;

// ItemId is injected from the navigation query string when navigating to this page
[QueryProperty(nameof(ItemId), "ItemId")]
public partial class ItemDetailViewModel : BaseViewModel
{
    private readonly IItemRepository _itemRepository;
    private readonly INavigationService _navigationService;
    private readonly IAuthenticationService _authService;

    [ObservableProperty]
    private int itemId;

    [ObservableProperty]
    private Item? item;

    // True when the logged-in user owns this item; the view binds this to show/hide the Edit button
    [ObservableProperty]
    private bool isOwner;

    public ItemDetailViewModel(IItemRepository itemRepository, INavigationService navigationService, IAuthenticationService authService)
    {
        _itemRepository = itemRepository;
        _navigationService = navigationService;
        _authService = authService;
        Title = "Item Details";
    }

    // Called automatically by the toolkit when ItemId is set via navigation
    partial void OnItemIdChanged(int value)
    {
        _ = LoadItemAsync();
    }

    [RelayCommand]
    private async Task LoadItemAsync()
    {
        if (ItemId <= 0) return;
        try
        {
            IsBusy = true;
            ClearError();
            Item = await _itemRepository.GetByIdAsync(ItemId);
            if (Item != null)
            {
                Title = Item.Title;
                // Compare logged-in user ID to item owner to decide if edit controls should appear
                IsOwner = _authService.CurrentUser?.Id == Item.OwnerId;
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load item: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Navigates to the rental flow, passing this item's ID as a query parameter
    [RelayCommand]
    private async Task RentItemAsync()
    {
        if (Item == null) return;
        await _navigationService.NavigateToAsync("RentalRequestPage",
            new Dictionary<string, object> { ["ItemId"] = Item.Id });
    }

    // Navigates to AddEditItemPage with the ID set, which puts that VM into edit mode
    [RelayCommand]
    private async Task EditItemAsync()
    {
        if (Item == null) return;
        await _navigationService.NavigateToAsync("AddEditItemPage",
            new Dictionary<string, object> { ["ItemId"] = Item.Id });
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigationService.NavigateBackAsync();
    }
}
