namespace ServiceBusManager.ViewModels;

public sealed partial class MessageViewModel : ViewModel
{
    private readonly IServiceBusService serviceBusService;

    private List<ServiceBusReceivedMessage> selectedMessages = new List<ServiceBusReceivedMessage>();

    public MessageViewModel(IServiceBusService serviceBusService)
    {
        this.serviceBusService = serviceBusService;

    }

    [ObservableProperty]
    private string? queueName;

    [ObservableProperty]
    private string? subscriptionName;

    [ObservableProperty]
    private string? displayName;

    [ObservableProperty]
    private bool isDeadLetter;

    [ObservableProperty]
    private bool isSubscription;

    [ObservableProperty]
    private bool hasMessages;

    public bool HasSelectedMessages => selectedMessages.Count > 0;
    public int NumberOfSelectedMessages => selectedMessages.Count;

    [ObservableProperty]
    private ObservableCollection<ServiceBusReceivedMessage> messages = new ObservableCollection<ServiceBusReceivedMessage>();

    public async Task LoadMessages(string queueName, bool showDeadLetter = false, bool isTopicSubscription = false)
    {
        try
        {
            IsDeadLetter = showDeadLetter;
            IsSubscription = isTopicSubscription;

            IsBusy = true;

            DisplayName = queueName;

            if (queueName.Contains("/"))
            {
                var split = queueName.Split("/");

                QueueName = split[1];
                SubscriptionName = split[0];
            }
            else
            {
                SubscriptionName = null;
                QueueName = queueName;
            }

            await LoadAndUpdate();

            selectedMessages.Clear();
            UpdateSelectedMessages();
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }


        IsBusy = false;
    }

    private async Task LoadAndUpdate()
    {
        if (QueueName == null)
        {
            return;
        }

        List<ServiceBusReceivedMessage>? messages;

        if (IsDeadLetter)
        {
            messages = await serviceBusService.PeekDeadLetter(QueueName, SubscriptionName);
        }
        else
        {
            messages = await serviceBusService.Peek(QueueName, SubscriptionName);
        }

        await Update(messages);
    }


    private async Task Update(List<ServiceBusReceivedMessage> messages)
    {
        if (!MainThread.IsMainThread)
        {
            await MainThread.InvokeOnMainThreadAsync(() => Update(messages));
            return;
        }

        Messages = new ObservableCollection<ServiceBusReceivedMessage>(messages);
    }

    [RelayCommand]
    private void ShowDetails(ServiceBusReceivedMessage? message)
    {
        if (message == null)
        {
            return;
        }

        (ServiceBusReceivedMessage Message, bool IsDeadLetter) parameter = (message, IsDeadLetter);

        AddAction($"update_messages", () =>
        {
            MainThread.BeginInvokeOnMainThread(async() => await Refresh());
        });

        RunAction($"open_{nameof(MessageDetailsView)}", parameter);
    }

    [RelayCommand]
    private void ToggleMessageSelected(ServiceBusReceivedMessage? message)
    {
        if (message == null)
        {
            return;
        }

        if (selectedMessages.Contains(message))
        {
            selectedMessages.Remove(message);
        }
        else
        {
            selectedMessages.Add(message);
        }

        UpdateSelectedMessages();
    }

    private void UpdateSelectedMessages()
    {
        OnPropertyChanged(nameof(HasSelectedMessages));
        OnPropertyChanged(nameof(NumberOfSelectedMessages));
    }

    [RelayCommand]
    private void ResendMessages()
    {
        if (currentQueueOrTopic == null || !HasSelectedMessages)
        {
            return;
        }

        IsBusy = true;

        Task.Run(async () =>
        {
            try
            {

                foreach (var message in selectedMessages)
                {
                    await serviceBusService.Resend(currentQueueOrTopic, message);
                }

                selectedMessages.Clear();
                UpdateSelectedMessages();

                await LoadAndUpdate();

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            MainThread.BeginInvokeOnMainThread(() => IsBusy = false);
        });

    }

    [RelayCommand]
    private void RemoveMessages()
    {
        if (currentQueueOrTopic == null || !HasSelectedMessages)
        {
            return;
        }

        IsBusy = true;

        Task.Run(async () =>
        {
            try
            {
                foreach (var message in selectedMessages)
                {
                    await serviceBusService.Remove(currentQueueOrTopic, IsDeadLetter, message);
                }

                selectedMessages.Clear();
                UpdateSelectedMessages();

                await LoadAndUpdate();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            MainThread.BeginInvokeOnMainThread(() => IsBusy = false);
        });
    }

    [RelayCommand]
    private async Task Refresh()
    {
        try
        {
            IsBusy = true;

            await LoadAndUpdate();

        }
        catch(Exception ex)
        {
            HandleException(ex);
        }

        IsBusy = false;
    }
}

