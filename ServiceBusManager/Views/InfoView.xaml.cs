namespace ServiceBusManager.Views;

public partial class InfoView : ContentView
{
    private readonly InfoViewModel? viewModel;
    private readonly ILogService logService;

    public static BindableProperty QueueNameProperty = BindableProperty.Create(nameof(QueueName), typeof(string), typeof(InfoView), null, defaultBindingMode: BindingMode.TwoWay,
       propertyChanged: (bindable, oldValue, newValue) =>
       {
           if (bindable is InfoView infoView && infoView.Content.BindingContext is InfoViewModel viewModel && newValue is string queueName)
           {
               MainThread.BeginInvokeOnMainThread(async () => await viewModel.Load(queueName));

               Task.Run(async () => await infoView.logService.LogPageView(nameof(InfoView)));
           }
       });

    public InfoView()
    {
        InitializeComponent();

        viewModel = Resolver.Resolve<InfoViewModel>();
        var log = Resolver.Resolve<ILogService>();

        if(log == null)
        {
            throw new Exception("ILogService need to be added to IoC");
        }

        logService = log;

        Content.BindingContext = viewModel;
    }

    public string? QueueName
    {
        get => GetValue(QueueNameProperty) as string;
        set => SetValue(QueueNameProperty, value);
    }
}
