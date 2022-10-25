using CommunityToolkit.Mvvm.Input;

namespace ServiceBusManager.ViewModels;

public sealed partial class MainViewModel : ViewModel
{
    private readonly IServiceBusService serviceBusService;

    public MainViewModel(IServiceBusService serviceBusService)
    {
        this.serviceBusService = serviceBusService;
    }

    public override async Task OnParameterSet()
    {
        await base.OnParameterSet();

        try
        {
            IsBusy = true;

            connectionString = NavigationParameter as string;

            if (connectionString == null)
            {
                return;
            }

            await serviceBusService.Init(connectionString);

            var queueTask = serviceBusService.GetQueues();
            var topicTask = serviceBusService.GetTopics();

            await Task.WhenAll(queueTask, topicTask);

            var queueResult = queueTask.Result;
            var topicResult = topicTask.Result;

            var queueGroup = new CollectionGroup<QueueOrTopic>("Queues", queueResult);
            var topicGroup = new CollectionGroup<QueueOrTopic>("Topics", topicResult);

            var items = new ObservableCollection<CollectionGroup<QueueOrTopic>>
                {
                    queueGroup,
                    topicGroup
                };

            Queues = new ObservableCollection<CollectionGroup<QueueOrTopic>>(items);
            ServiceBusNamespace = await serviceBusService.GetNamepspace();

        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
        finally
        {
            IsBusy = false;
        }

    }


    [ObservableProperty]
    private bool showPeek;

    [ObservableProperty]
    private bool showInfo;

    [ObservableProperty]
    private bool showDeadLetters;

    [ObservableProperty]
    private bool showDetails;

    [ObservableProperty]
    private ObservableCollection<CollectionGroup<QueueOrTopic>> queues = new();

    [ObservableProperty]
    private string? connectionString;

    [ObservableProperty]
    private string? serviceBusNamespace;

    [ObservableProperty]
    private string? currentQueue;

    [ObservableProperty]
    private string? currentDeadLetterQueue;


    [RelayCommand]
    private void OpenInfo(string? queue)
    {
        if (string.IsNullOrWhiteSpace(queue))
        {
            return;
        }

        ShowInfo = true;
        ShowPeek = false;
        ShowDeadLetters = false;

        CurrentQueue = queue;
    }

    [RelayCommand]
    private void OpenPeek(string? queue)
    {
        if (string.IsNullOrWhiteSpace(queue))
        {
            return;
        }

        CurrentQueue = queue;
        currentQueueOrTopic = queue;

        ShowInfo = false;
        ShowPeek = true;
        ShowDeadLetters = false;
    }

    [RelayCommand]
    private void OpenDeadLetters(string? queue)
    {
        if (string.IsNullOrWhiteSpace(queue))
        {
            return;
        }
        CurrentDeadLetterQueue = queue;
        currentQueueOrTopic = queue;

        ShowInfo = false;
        ShowPeek = false;
        ShowDeadLetters = true;
    }
}


