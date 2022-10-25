using CommunityToolkit.Mvvm.Input;

namespace ServiceBusManager.ViewModels;

public sealed partial class MessageDetailsViewModel : ViewModel
{
    public MessageDetailsViewModel(IServiceBusService serviceBusService)
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
    private bool isEditMode;

    [ObservableProperty]
    private bool isDeadLetter;

    [ObservableProperty]
    private string? raw;

    [ObservableProperty]
    private string? body;

    private readonly IServiceBusService serviceBusService;

    public void LoadMessage(ServiceBusReceivedMessage message, bool isDeadLetterQueue)
    {
        Item = message;
        IsDeadLetter = isDeadLetterQueue;

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

        Task.Run(async () => await serviceBusService.Resend(currentQueueOrTopic, Item));

        Close();
    }

    [RelayCommand]
    private void Remove()
    {
        if (currentQueueOrTopic == null || Item == null)
        {
            return;
        }

        Task.Run(async () => await serviceBusService.Remove(currentQueueOrTopic, IsDeadLetter, Item));

        Close();
    }
}

