using ServiceBusManager.Services;

namespace ServiceBusManager.Views;

public partial class MessageDetailsView : ContentView
{
    private readonly MessageDetailsViewModel? viewModel;

    public MessageDetailsView()
	{
		InitializeComponent();

		 viewModel = Resolver.Resolve<MessageDetailsViewModel>();

        BindingContext = viewModel;

    }
}
