using ServiceBusManager.Services;

namespace ServiceBusManager.Views;

public partial class DeadLettersView
{
    private readonly ILogService logService;

    public DeadLettersView(DeadLettersViewModel viewModel, ILogService logService)
	{
		InitializeComponent();

		BindingContext = viewModel;
        this.logService = logService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Task.Run(async () => await logService.LogPageView(nameof(DeadLettersView)));
    }
}
