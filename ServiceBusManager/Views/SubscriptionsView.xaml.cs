namespace ServiceBusManager.Views;

public partial class SubscriptionsView : ContentView
{
    private readonly SubscriptionsViewModel? viewModel;

    public static BindableProperty TopicNameProperty = BindableProperty.Create(nameof(TopicName), typeof(string), typeof(SubscriptionsView), null, defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            if (bindable is SubscriptionsView view && view.MainContent.BindingContext is SubscriptionsViewModel viewModel && newValue is string topicName)
            {
                MainThread.BeginInvokeOnMainThread(async () => await viewModel.LoadSubscriptions(topicName));
            }
        });

    public SubscriptionsView()
	{
		InitializeComponent();

        viewModel = Resolver.Resolve<SubscriptionsViewModel>();

        MainContent.BindingContext = viewModel;

    }

    public string? TopicName
    {
        get => GetValue(TopicNameProperty) as string;
        set => SetValue(TopicNameProperty, value);
    }
}
