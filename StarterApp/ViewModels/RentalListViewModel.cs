using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Models;
using StarterApp.Repositories;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class RentalListViewModel : BaseViewModel
{
    private readonly IRentalRepository _rentalRepository;
    private readonly INavigationService _navigationService;
    // Added so we can check whether the user's session has expired before loading data
    private readonly IAuthenticationService _authService;

    // Items being rented out by the current user
    [ObservableProperty]
    private ObservableCollection<Rental> incomingRentals = new();

    // Items the current user is renting from others
    [ObservableProperty]
    private ObservableCollection<Rental> outgoingRentals = new();

    // Bound to the pull-to-refresh control; reset to false once loading finishes
    [ObservableProperty]
    private bool isRefreshing;

    // Tab toggle: true = show incoming list, false = show outgoing list
    [ObservableProperty]
    private bool showIncoming = true;

    public RentalListViewModel(IRentalRepository rentalRepository, INavigationService navigationService, IAuthenticationService authService)
    {
        _rentalRepository = rentalRepository;
        _navigationService = navigationService;
        _authService = authService;
        Title = "My Rentals";
    }

    [RelayCommand]
    private async Task LoadRentalsAsync()
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
            var incoming = await _rentalRepository.GetIncomingAsync();
            var outgoing = await _rentalRepository.GetOutgoingAsync();

            // Replace collection contents without reassigning, so UI bindings stay intact
            IncomingRentals.Clear();
            foreach (var r in incoming)
                IncomingRentals.Add(r);

            OutgoingRentals.Clear();
            foreach (var r in outgoing)
                OutgoingRentals.Add(r);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load rentals: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false; // always stop the spinner, even on error
        }
    }

    // Sets IsRefreshing first so the pull-to-refresh spinner appears before data loads
    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadRentalsAsync();
    }

    // Tab commands flip the single ShowIncoming flag; the view binds list visibility to it
    [RelayCommand]
    private void ShowIncomingTab() => ShowIncoming = true;

    [RelayCommand]
    private void ShowOutgoingTab() => ShowIncoming = false;

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigationService.NavigateBackAsync();
    }

    // Entry point called by the page after navigation
    public async Task InitializeAsync()
    {
        await LoadRentalsAsync();
    }
}
