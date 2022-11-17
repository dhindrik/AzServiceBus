using CommunityToolkit.Mvvm.Input;

namespace ServiceBusManager.ViewModels;

public sealed partial class MainViewModel : ViewModel
{
    private readonly IServiceBusService serviceBusService;
    private readonly IConnectionService connectionService;

    public MainViewModel(IServiceBusService serviceBusService, IConnectionService connectionService, ILogService logService) : base(logService)
    {
        this.serviceBusService = serviceBusService;
        this.connectionService = connectionService;
    }

    public override async Task OnParameterSet()
    {
        await base.OnParameterSet();

        try
        {
            IsBusy = true;

            if (NavigationParameter is string)
            {
                connectionString = NavigationParameter as string;
            }
            else if(NavigationParameter is DeadLetterInfo info)
            {
                var connections = await connectionService.Get();

                connectionString = connections.First(x => x.Name == info.Connection).Value;
                OpenDeadLetters(info.Fullname);
            }

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
    private bool showSubscriptions;

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

    [ObservableProperty]
    private string? currentTopic;

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
        ShowSubscriptions = false;

        CurrentQueue = queue;
    }

    [RelayCommand]
    private void OpenTopicInfo(string? queue)
    {
        if (string.IsNullOrWhiteSpace(queue))
        {
            return;
        }

        ShowInfo = true;
        ShowPeek = false;
        ShowDeadLetters = false;
        ShowSubscriptions = false;

        CurrentTopic = queue;
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
        ShowSubscriptions = false;
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
        ShowSubscriptions = false;
    }

    [RelayCommand]
    private void OpenSubscriptions(string? topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
        {
            return;
        }

        AddAction("OpenPeek", (topic) => {
            OpenPeek($"{CurrentTopic}/{topic.ToString()}");

            RemoveAction("OpenPeek");
        });

        AddAction("OpenDeadLetters", (topic) => {
            OpenDeadLetters($"{CurrentTopic}/{topic.ToString()}");

            RemoveAction("OpenDeadLetters");
        });

        CurrentTopic = topic;

        ShowInfo = false;
        ShowPeek = false;
        ShowDeadLetters = false;
        ShowSubscriptions = true;
    }
}


