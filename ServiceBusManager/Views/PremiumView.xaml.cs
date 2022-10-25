namespace ServiceBusManager.Views;

public partial class PremiumView : ContentPage
{
    private readonly PremiumViewModel viewModel;

    public PremiumView(PremiumViewModel viewModel)
	{
		InitializeComponent();
        this.viewModel = viewModel;

        BindingContext = viewModel;
    }
}
