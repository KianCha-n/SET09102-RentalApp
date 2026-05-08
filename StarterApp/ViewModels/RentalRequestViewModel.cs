using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Models;
using StarterApp.Repositories;
using StarterApp.Services;

namespace StarterApp.ViewModels;

[QueryProperty(nameof(ItemId), "ItemId")]
public partial class RentalRequestViewModel : BaseViewModel
{
    private readonly IItemRepository _itemRepository;
    private readonly IRentalRepository _rentalRepository;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private int itemId;

    [ObservableProperty]
    private Item? item;

    [ObservableProperty]
    private DateTime startDate = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private DateTime endDate = DateTime.Today.AddDays(2);

    [ObservableProperty]
    private decimal estimatedCost;

    public RentalRequestViewModel(IItemRepository itemRepository, IRentalRepository rentalRepository, INavigationService navigationService)
    {
        _itemRepository = itemRepository;
        _rentalRepository = rentalRepository;
        _navigationService = navigationService;
        Title = "Request Rental";
    }

    partial void OnItemIdChanged(int value)
    {
        _ = LoadItemAsync();
    }

    partial void OnStartDateChanged(DateTime value) => UpdateEstimatedCost();
    partial void OnEndDateChanged(DateTime value) => UpdateEstimatedCost();

    private async Task LoadItemAsync()
    {
        if (ItemId <= 0) return;
        try
        {
            IsBusy = true;
            Item = await _itemRepository.GetByIdAsync(ItemId);
            UpdateEstimatedCost();
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

    private void UpdateEstimatedCost()
    {
        if (Item == null) return;
        var days = (EndDate - StartDate).Days;
        EstimatedCost = days > 0 ? days * Item.DailyRate : 0;
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (EndDate <= StartDate)
        {
            SetError("End date must be after start date");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();
            await _rentalRepository.CreateAsync(new RentalRequest
            {
                ItemId = ItemId,
                StartDate = StartDate.ToString("yyyy-MM-dd"),
                EndDate = EndDate.ToString("yyyy-MM-dd")
            });
            await Application.Current!.MainPage!.DisplayAlert("Success", "Rental request submitted!", "OK");
            await _navigationService.NavigateBackAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to submit: {ex.Message}");
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
