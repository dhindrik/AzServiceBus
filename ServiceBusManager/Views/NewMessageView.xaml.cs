namespace ServiceBusManager.Views;

public partial class NewMessageView : ContentView
{
    private readonly NewMessageViewModel? viewModel;
    private readonly ILogService logService;

    public NewMessageView()
    {
        InitializeComponent();

        viewModel = Resolver.Resolve<NewMessageViewModel>();

        BindingContext = viewModel;

        var log = Resolver.Resolve<ILogService>();

        if (log == null)
        {
            throw new Exception("ILogService need to be added to IoC");
        }

        logService = log;
    }

    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(IsVisible) && IsVisible)
        {
            Task.Run(async () => await logService.LogPageView(nameof(NewMessageViewModel)));
        }
    }
}
