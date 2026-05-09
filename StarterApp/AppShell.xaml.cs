using StarterApp.ViewModels;
using StarterApp.Views;

namespace StarterApp;


/// The AppShell class defines the main navigation structure of the app using Shell. 
/// It registers routes for all the pages in the app and sets up the initial view model for data binding. 
/// The Shell provides a consistent navigation experience across different platforms and allows for easy page transitions.

public partial class AppShell : Shell
{
	public AppShell(AppShellViewModel viewModel)
	{
		BindingContext = viewModel;
		InitializeComponent();
		RegisterRoutes();
	}

	private static void RegisterRoutes()
	{
		Routing.RegisterRoute("MainPage", typeof(MainPage));
		Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
		Routing.RegisterRoute("ItemListPage", typeof(ItemListPage));
		Routing.RegisterRoute("ItemDetailPage", typeof(ItemDetailPage));
		Routing.RegisterRoute("AddEditItemPage", typeof(AddEditItemPage));
		Routing.RegisterRoute("RentalListPage", typeof(RentalListPage));
		Routing.RegisterRoute("RentalRequestPage", typeof(RentalRequestPage));
		Routing.RegisterRoute("UserListPage", typeof(UserListPage));
		Routing.RegisterRoute("UserDetailPage", typeof(UserDetailPage));
	}
}
