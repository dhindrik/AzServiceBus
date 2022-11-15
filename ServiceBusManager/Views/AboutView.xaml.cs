namespace ServiceBusManager.Views;

public partial class AboutView : ContentPage
{
    private readonly AboutViewModel viewModel;
    private readonly ILogService logService;

    public AboutView(AboutViewModel viewModel, ILogService logService)
	{
		InitializeComponent();
        this.viewModel = viewModel;
        this.logService = logService;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Task.Run(async() => await logService.LogPageView(nameof(AboutView)));
    }
}
