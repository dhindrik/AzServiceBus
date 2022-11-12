namespace ServiceBusManager.ViewModels;

public partial class SubscriptionsViewModel : ViewModel
{
    private readonly IServiceBusService serviceBusService;

    public SubscriptionsViewModel(IServiceBusService serviceBusService)
    {
        this.serviceBusService = serviceBusService;
    }

    [ObservableProperty]
    private string? topicName;

    [ObservableProperty]
    private ObservableCollection<Subscription> items = new ();

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

    private async Task LoadData()
    {
        if(TopicName == null)
        {
            return;
        }

        var subscriptions = await serviceBusService.GetSubscriptions(TopicName);

        Items = new ObservableCollection<Subscription>(subscriptions);
    }
}

