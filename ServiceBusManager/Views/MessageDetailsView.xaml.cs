namespace ServiceBusManager.Views;

public partial class MessageDetailsView : ContentView
{
	public MessageDetailsView()
	{
		InitializeComponent();

		BindingContext = Resolver.Resolve<MessageDetailsViewModel>();
	}
}
