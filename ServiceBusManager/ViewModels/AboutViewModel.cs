
using CommunityToolkit.Mvvm.Input;

namespace ServiceBusManager.ViewModels;

public sealed partial class AboutViewModel : ViewModel
{
    public AboutViewModel()
    {
        AppVersion = $"{VersionTracking.CurrentVersion}.{VersionTracking.CurrentBuild}";
    }

    [ObservableProperty]
    private string appVersion = string.Empty;

    [RelayCommand]
    private async Task Source()
    {
        var uri = new Uri("https://github.com/dhindrik/AzServiceBus");
        await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
    }

    [RelayCommand]
    private async Task Privacy()
    {
        var uri = new Uri("https://github.com/dhindrik/AzServiceBus/blob/main/PrivacyPolicy.md");
        await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
    }
}

