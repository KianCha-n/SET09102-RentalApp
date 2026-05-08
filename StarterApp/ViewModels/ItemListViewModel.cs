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

    [ObservableProperty]
    private ObservableCollection<Item> items = new();

    [ObservableProperty]
    private bool isRefreshing;

    public ItemListViewModel(IItemRepository itemRepository, INavigationService navigationService)
    {
        _itemRepository = itemRepository;
        _navigationService = navigationService;
        Title = "Browse Items";
    }

    [RelayCommand]
    private async Task LoadItemsAsync()
    {
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
