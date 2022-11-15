using ServiceBusManager.Services;

namespace ServiceBusManager.Views;

public partial class PremiumView
{
    private readonly PremiumViewModel viewModel;
    private readonly ILogService logService;

    public PremiumView(PremiumViewModel viewModel, ILogService logService)
	{
		InitializeComponent();
        this.viewModel = viewModel;
        this.logService = logService;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Task.Run(async () => await logService.LogPageView(nameof(PremiumView)));
    }
}
