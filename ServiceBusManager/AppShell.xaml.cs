namespace ServiceBusManager;

public partial class AppShell : Shell
{
	public AppShell(ShellViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	
	}
}

