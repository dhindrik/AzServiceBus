namespace ServiceBusManager.ViewModels;

public sealed partial class MessageDetailsViewModel : ViewModel
{
    private readonly IServiceBusService serviceBusService;

    private string? topicName;

    public MessageDetailsViewModel(IServiceBusService serviceBusService, ILogService logService) : base(logService)
    {
        this.serviceBusService = serviceBusService;

    }

    [ObservableProperty]
    private ServiceBusReceivedMessage? item;

    public bool IsFormatted => !IsRaw;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormatted))]
    private bool isRaw;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsReadMode))]
    private bool isEditMode;

    public bool IsReadMode => !isEditMode;

    [ObservableProperty]
    private bool isDeadLetter;

    [ObservableProperty]
    private string? raw;

    [ObservableProperty]
    private string? body;


    public void LoadMessage(ServiceBusReceivedMessage message, bool isDeadLetterQueue, string? topicName)
    {
        Item = message;
        IsDeadLetter = isDeadLetterQueue;

        this.topicName = topicName;

        IsEditMode = false;
        IsRaw = false;

        var json = JsonSerializer.Serialize(message);
        Raw = json;

        Body = message.Body.ToString();

       
    }

    [RelayCommand]
    private void Close()
    {
        RunAction("update_messages");
        RemoveAction("update_messages");
        RunAction($"close_{nameof(MessageDetailsView)}");        
    }

    [RelayCommand]
    private void Edit()
    {
        IsEditMode = !IsEditMode;

        if (IsEditMode)
        {
            IsRaw = false;
        }
    }

    [RelayCommand]
    private void Resend()
    {
        if (currentQueueOrTopic == null || Item == null)
        {
            return;
        }

        if(isEditMode)
        {
            Task.Run(async () => await serviceBusService.Resend(currentQueueOrTopic, Item,Body, topicName));
        }
        else
        {
            Task.Run(async () => await serviceBusService.Resend(currentQueueOrTopic, Item,null, topicName));
        }
        

        Close();
    }

    [RelayCommand]
    private void Remove()
    {
        if (currentQueueOrTopic == null || Item == null)
        {
            return;
        }

        Task.Run(async () => await serviceBusService.Remove(currentQueueOrTopic, IsDeadLetter, Item, topicName));

        Close();
    }
}

