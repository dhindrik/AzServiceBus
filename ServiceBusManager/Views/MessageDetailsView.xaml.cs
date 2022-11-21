using System.Runtime.CompilerServices;
using ServiceBusManager.Services;

namespace ServiceBusManager.Views;

public partial class MessageDetailsView : ContentView
{
    private readonly MessageDetailsViewModel? viewModel;
    private readonly ILogService logService;

    public MessageDetailsView()
	{
		InitializeComponent();

		 viewModel = Resolver.Resolve<MessageDetailsViewModel>();

        BindingContext = viewModel;

        var log = Resolver.Resolve<ILogService>();

        if (log == null)
        {
            throw new Exception("ILogService need to be added to IoC");
        }

        logService = log;
    }

    protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(IsVisible) && IsVisible)
        {
            Task.Run(async () => await logService.LogPageView(nameof(MessageDetailsView)));
        }
    }
}
