namespace ServiceBusManager.Views;

public partial class InfoView : ContentView
{
    private readonly InfoViewModel? viewModel;

    public static BindableProperty QueueNameProperty = BindableProperty.Create(nameof(QueueName), typeof(string), typeof(InfoView), null, defaultBindingMode: BindingMode.TwoWay,
       propertyChanged: (bindable, oldValue, newValue) =>
       {
           if (bindable is InfoView infoView && infoView.Content.BindingContext is InfoViewModel viewModel && newValue is string queueName)
           {
               MainThread.BeginInvokeOnMainThread(async () => await viewModel.Load(queueName));
           }
       });

    public InfoView()
    {
        InitializeComponent();

        viewModel = Resolver.Resolve<InfoViewModel>();

        Content.BindingContext = viewModel;
    }

    public string? QueueName
    {
        get => GetValue(QueueNameProperty) as string;
        set => SetValue(QueueNameProperty, value);
    }
}
