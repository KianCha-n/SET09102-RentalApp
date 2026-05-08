using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class AddEditItemPage : ContentPage
{
    private readonly AddEditItemViewModel _viewModel;

    public AddEditItemPage(AddEditItemViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
