using ServiceBusManager.ViewModels;

namespace ServiceBusManager.Views;

public partial class ConnectView
{
	public ConnectView(ConnectViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	}
}
