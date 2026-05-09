using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Models;
using StarterApp.Repositories;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class ItemListViewModel : BaseViewModel
{
    private readonly IItemRepository _itemRepository;
    private readonly INavigationService _navigationService;
    // Added so we can check whether the user's session has expired before loading data
    private readonly IAuthenticationService _authService;

    [ObservableProperty]
    private ObservableCollection<Item> items = new();

    [ObservableProperty]
    private bool isRefreshing;

    public ItemListViewModel(IItemRepository itemRepository, INavigationService navigationService, IAuthenticationService authService)
    {
        _itemRepository = itemRepository;
        _navigationService = navigationService;
        _authService = authService;
        Title = "Browse Items";
    }

    [RelayCommand]
    private async Task LoadItemsAsync()
    {
        // If the API token has expired, clear the session and send the user back to login
        if (_authService.IsTokenExpired)
        {
            await _authService.LogoutAsync();
            await _navigationService.NavigateToAsync("//LoginPage");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();
            var result = await _itemRepository.GetAllAsync();
            Items.Clear();
            foreach (var item in result)
                Items.Add(item);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load items: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadItemsAsync();
    }

    [RelayCommand]
    private async Task SelectItemAsync(Item item)
    {
        if (item == null) return;
        await _navigationService.NavigateToAsync("ItemDetailPage",
            new Dictionary<string, object> { ["ItemId"] = item.Id });
    }

    [RelayCommand]
    private async Task AddItemAsync()
    {
        await _navigationService.NavigateToAsync("AddEditItemPage");
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigationService.NavigateBackAsync();
    }

    public async Task InitializeAsync()
    {
        await LoadItemsAsync();
    }
}
