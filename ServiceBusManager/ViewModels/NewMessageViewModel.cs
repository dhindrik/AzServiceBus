using System;
namespace ServiceBusManager.ViewModels;

public sealed partial class NewMessageViewModel : ViewModel
{
    private readonly ILogService logService;
    private readonly IServiceBusService serviceBusService;

    public NewMessageViewModel(ILogService logService, IServiceBusService serviceBusService) : base(logService)
    {
        this.logService = logService;
        this.serviceBusService = serviceBusService;
    }

    public override async Task Initialize()
    {
        await base.Initialize();

        Properties.Add(Resolver.Resolve<PropertyViewModel>()!);
        Properties.Add(Resolver.Resolve<PropertyViewModel>()!);
    }

    [ObservableProperty]
    private List<string> contentTypes = new List<string>() { "application/json", "text/plain" };

    [ObservableProperty]
    private string? contentType = "application/json";

    [ObservableProperty]
    private string? message;

    [ObservableProperty]
    private ObservableCollection<PropertyViewModel> properties = new();

    [RelayCommand]
    private void AddProperty()
    {
        Properties.Add(Resolver.Resolve<PropertyViewModel>()!);
    }

    [RelayCommand]
    private void RemoveProperty(PropertyViewModel property)
    {
        Properties.Remove(property);

        if(properties.Count == 0)
        {
            Properties.Add(Resolver.Resolve<PropertyViewModel>()!);
        }
    }

    [RelayCommand]
    private void Close()
    {
        RunAction("update_messages");
        RemoveAction("update_messages");
        RunAction($"close_{nameof(NewMessageView)}");

        Properties.Clear();
        ContentType = null;
        Message = null;
    }

    [RelayCommand]
    private async Task Send()
    {
        try
        {
            IsBusy = true;

            var message = new ServiceBusMessage(Message)
            {
                ContentType = ContentType
            };

            var props = Properties.Where(x => !string.IsNullOrWhiteSpace(x.Key)).ToList();

            if(props.Count > 0)
            {
                foreach(var prop in props)
                {
                    message.ApplicationProperties.Add(prop.Key, prop.Value);
                }
            }

            if (currentQueueOrTopic != null && currentQueueOrTopic.Contains("/"))
            {
                var split = currentQueueOrTopic.Split("/");

                await serviceBusService.Send(split[1], message, split[0]);
            }
            else if (currentQueueOrTopic != null)
            {
                await serviceBusService.Send(currentQueueOrTopic, message);
            }

            var toast = Toast.Make($"Message is sent to '{currentQueueOrTopic}");
            await toast.Show();

            Close();
        }
        catch (Exception ex)
        {
            await logService.LogException(ex);
        }        

        IsBusy = false;
    }
}

public sealed partial class PropertyViewModel : ViewModel
{
    public PropertyViewModel(ILogService logService, NewMessageViewModel messageViewModel) : base(logService)
    {
        messageViewModel.Properties.CollectionChanged += Properties_CollectionChanged;
    }

    [ObservableProperty]
    private string? key;

    [ObservableProperty]
    private string? value;

    [ObservableProperty]
    private bool isLast;

    private void Properties_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        var collection = sender as ObservableCollection<PropertyViewModel>;

        if (collection != null)
        {
            IsLast = collection.LastOrDefault() == this;
        }


    }
}


