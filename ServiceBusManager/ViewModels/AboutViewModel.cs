
using CommunityToolkit.Mvvm.Input;

namespace ServiceBusManager.ViewModels;

public sealed partial class AboutViewModel : ViewModel
{
    public AboutViewModel()
    {
    }

    [RelayCommand]
    private async Task Source()
    {
        var uri = new Uri("https://github.com/dhindrik/ServiceBusManager");
        await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
    }

    [RelayCommand]
    private async Task Privacy()
    {
        var uri = new Uri("https://github.com/dhindrik/ServiceBusManager/blob/main/PrivacyPolicy.md");
        await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
    }
}

