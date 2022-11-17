namespace ServiceBusManager.ViewModels;

public sealed partial class SubscriptionsViewModel : ViewModel
{
    private readonly IServiceBusService serviceBusService;

    public SubscriptionsViewModel(IServiceBusService serviceBusService, ILogService logService) : base(logService)
    {
        this.serviceBusService = serviceBusService;
    }

    [ObservableProperty]
    private string? topicName;

    [ObservableProperty]
    private ObservableCollection<Subscription> items = new();

    public async Task LoadSubscriptions(string topic)
    {
        try
        {
            IsBusy = true;

            TopicName = topic;

            await LoadData();
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }

        IsBusy = false;
    }

    [RelayCommand]
    private async Task Refresh()
    {
        IsBusy = true;

        await LoadData();

        IsBusy = false;
    }

    [RelayCommand]
    private void Peek(Subscription subscription)
    {
        if (subscription.Name == null)
        {
            return;
        }

        RunAction("OpenPeek", subscription.Name);
    }

    [RelayCommand]
    private void DeadLetters(Subscription subscription)
    {
        if (subscription.Name == null)
        {
            return;
        }

        RunAction("DeadLetters", subscription.Name);
    }

    private async Task LoadData()
    {
        if (TopicName == null)
        {
            return;
        }

        var subscriptions = await serviceBusService.GetSubscriptions(TopicName);

        Items = new ObservableCollection<Subscription>(subscriptions);
    }
}

