namespace ServiceBusManager.Views;

public partial class AboutView : ContentPage
{
    private readonly AboutViewModel viewModel;

    public AboutView(AboutViewModel viewModel)
	{
		InitializeComponent();
        this.viewModel = viewModel;

        BindingContext = viewModel;
    }
}
