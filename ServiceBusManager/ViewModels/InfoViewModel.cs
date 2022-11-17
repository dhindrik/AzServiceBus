namespace ServiceBusManager.ViewModels;

public sealed partial class InfoViewModel : ViewModel
{
    private readonly IServiceBusService serviceBusService;

    public InfoViewModel(IServiceBusService serviceBusService, ILogService logService) : base(logService)
    {
        this.serviceBusService = serviceBusService;
    }

    [ObservableProperty]
    private QueueOrTopic? item;

    public async Task Load(string queueName)
    {
        try
        {
            IsBusy = true;
            Item = await serviceBusService.GetQueue(queueName);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }

        IsBusy = false;
    }

    public async Task LoadTopic(string topicName)
    {
        try
        {
            IsBusy = true;
            Item = await serviceBusService.GetTopic(topicName);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }

        IsBusy = false;
    }
}

