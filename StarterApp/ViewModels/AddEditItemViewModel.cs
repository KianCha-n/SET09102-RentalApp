using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Models;
using StarterApp.Repositories;
using StarterApp.Services;

namespace StarterApp.ViewModels;

// ItemId is injected from the navigation query string (e.g. ?ItemId=5); 0 means "new item"
[QueryProperty(nameof(ItemId), "ItemId")]
public partial class AddEditItemViewModel : BaseViewModel
{
    private readonly IItemRepository _itemRepository;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private int itemId;

    [ObservableProperty]
    private string itemTitle = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    // Stored as text so the user can type freely; parsed to decimal on save
    [ObservableProperty]
    private string dailyRateText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Category> categories = new();

    [ObservableProperty]
    private Category? selectedCategory;

    [ObservableProperty]
    private bool isEditMode;

    public AddEditItemViewModel(IItemRepository itemRepository, INavigationService navigationService)
    {
        _itemRepository = itemRepository;
        _navigationService = navigationService;
        Title = "Add Item";
    }

    // Called automatically by the toolkit when ItemId is set via navigation
    partial void OnItemIdChanged(int value)
    {
        if (value > 0)
        {
            IsEditMode = true;
            Title = "Edit Item";
            _ = LoadItemAsync();
        }
    }

    // Entry point called by the page after navigation; loads categories then item data if editing
    public async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
        if (IsEditMode)
            await LoadItemAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var cats = await _itemRepository.GetCategoriesAsync();
            Categories.Clear();
            foreach (var c in cats)
                Categories.Add(c);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load categories: {ex.Message}");
        }
    }

    // Fetches the existing item and pre-fills form fields for editing
    private async Task LoadItemAsync()
    {
        try
        {
            IsBusy = true;
            var item = await _itemRepository.GetByIdAsync(ItemId);
            if (item != null)
            {
                ItemTitle = item.Title;
                Description = item.Description ?? string.Empty;
                DailyRateText = item.DailyRate.ToString("F2");
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == item.CategoryId);
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

    [RelayCommand]
    private async Task SaveAsync()
    {
        // Validate inputs before touching the repository
        if (string.IsNullOrWhiteSpace(ItemTitle) || ItemTitle.Length < 5)
        {
            SetError("Title must be at least 5 characters");
            return;
        }
        if (!decimal.TryParse(DailyRateText, out var rate) || rate <= 0 || rate > 1000)
        {
            SetError("Enter a valid daily rate (£0.01 – £1000)");
            return;
        }
        if (SelectedCategory == null)
        {
            SetError("Please select a category");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();
            var item = new Item
            {
                Id = ItemId,
                Title = ItemTitle.Trim(),
                Description = Description.Trim(),
                DailyRate = rate,
                CategoryId = SelectedCategory.Id,
                IsAvailable = true
            };

            // Route to create or update depending on mode
            if (IsEditMode)
                await _itemRepository.UpdateAsync(ItemId, item);
            else
                await _itemRepository.CreateAsync(item);

            await _navigationService.NavigateBackAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to save: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _navigationService.NavigateBackAsync();
    }
}
