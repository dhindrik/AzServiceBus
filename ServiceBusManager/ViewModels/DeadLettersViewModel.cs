using System.Linq;
using System.Text.RegularExpressions;


#if MACCATALYST
using UIKit;
using UserNotifications;
using Foundation;
#endif

namespace ServiceBusManager.ViewModels;

public sealed partial class DeadLettersViewModel : ViewModel
{
    private readonly IServiceBusService serviceBusService;

    public DeadLettersViewModel(IServiceBusService serviceBusService, ILogService logService) : base(logService)
    {
        this.serviceBusService = serviceBusService;
    }

    public override async Task Initialize()
    {
        await base.Initialize();

        try
        {
            IsBusy = true;

            await LoadData();

            GetNotifications = Preferences.Default.Get<bool>("Notifications", false);

#if MACCATALYST
            var settings = await UNUserNotificationCenter.Current.GetNotificationSettingsAsync();

            NotificationsNotAllowed = settings.AuthorizationStatus == UNAuthorizationStatus.Denied;
#endif
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }

        IsBusy = false;
    }

    public override async Task OnAppearing()
    {
        await base.OnAppearing();

        await Refresh();
    }

    private async Task LoadData()
    {
        var items = await serviceBusService.GetDeadLetters();

        List<CollectionGroup<DeadLetterInfo>> groups = new();

        Dictionary<string, List<DeadLetterInfo>> info = new();

        foreach (var item in items)
        {
            if (!info.ContainsKey(item.Key))
            {
                info.Add(item.Key, new List<DeadLetterInfo>());
            }

            var group = info[item.Key];

            foreach (var value in item.Value)
            {
                if (value.Name.Contains("/"))
                {
                    var split = value.Name.Split("/");

                    group.Add(new DeadLetterInfo(split[1], value.Count, split[0]) { Connection = item.Key});
                }
                else
                {
                    group.Add(new DeadLetterInfo(value.Name, value.Count) { Connection = item.Key });
                }
            }
        }

        foreach (var item in info.OrderBy(x => x.Key))
        {
            groups.Add(new CollectionGroup<DeadLetterInfo>(item.Key, item.Value));
        }

        Items = new ObservableCollection<CollectionGroup<DeadLetterInfo>>(groups);
    }

    [ObservableProperty]
    private ObservableCollection<CollectionGroup<DeadLetterInfo>> items = new();

    [ObservableProperty]
    private bool getNotifications;

    [ObservableProperty]
    private bool notificationsNotAllowed;

    [RelayCommand]
    public async Task ShowPremium()
    {
        await Navigation.NavigateTo("///Premium");
    }

    [RelayCommand]
    public async Task Show(DeadLetterInfo info)
    {
        await Navigation.NavigateTo($"///{nameof(MainViewModel)}", info);
    }

    [RelayCommand]
    public async Task Refresh()
    {
        await base.Initialize();

        try
        {
            IsBusy = true;

            await LoadData();

#if MACCATALYST
            var settings = await UNUserNotificationCenter.Current.GetNotificationSettingsAsync();

            NotificationsNotAllowed = settings.AuthorizationStatus == UNAuthorizationStatus.Denied;
#endif
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }

        IsBusy = false;
    }

    partial void OnGetNotificationsChanged(bool value)
    {
        if(IsInitialized)
        {
            _ = Task.Run(() => Preferences.Default.Set("Notifications", value));
        }
    }
}

