using CommunityToolkit.Mvvm.Input;

namespace ServiceBusManager.ViewModels;

public sealed partial class ShellViewModel : ViewModel
{
    public ShellViewModel(ILogService logService) : base(logService)
    {
    }

    [RelayCommand]
    private async Task Feedback()
    {
        var uri = new Uri("https://github.com/dhindrik/ServiceBusManager/issues");
        await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
    }
}

