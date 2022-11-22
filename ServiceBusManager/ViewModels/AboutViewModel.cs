namespace ServiceBusManager.ViewModels;

public sealed partial class AboutViewModel : ViewModel
{
    public AboutViewModel(ILogService logService) : base(logService)
    {
        AppVersion = $"{VersionTracking.CurrentVersion}.{VersionTracking.CurrentBuild}";
#if DEBUG
       LogPath = Path.Combine(FileSystem.AppDataDirectory, "AzServiceBus-DEBUG", "logs");
#else
       LogPath = Path.Combine(FileSystem.AppDataDirectory, "logs");
#endif
    }

    [ObservableProperty]
    private string appVersion = string.Empty;

    [ObservableProperty]
    private string logPath = string.Empty;

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
    [RelayCommand]
    private async Task CopyLogPath()
    {
        await Clipboard.Default.SetTextAsync(LogPath);

        var toast = Toast.Make("Log path has been copied to clipboard");

        await toast.Show();
    }
        
}

