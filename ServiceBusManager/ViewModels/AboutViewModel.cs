namespace ServiceBusManager.ViewModels;

public sealed partial class AboutViewModel : ViewModel
{
    public AboutViewModel(ILogService logService) : base(logService)
    {
        AppVersion = $"{VersionTracking.CurrentVersion}.{VersionTracking.CurrentBuild}";
#if DEBUG
       logPath = Path.Combine(FileSystem.AppDataDirectory, "AzServiceBus-DEBUG", "logs");
#else
       logPath = Path.Combine(FileSystem.AppDataDirectory, "logs");
#endif

        this.logService = logService;
    }

    [ObservableProperty]
    private string appVersion = string.Empty;

    private string logPath = string.Empty;
    private readonly ILogService logService;

    [RelayCommand]
    private async Task Source()
    {
        try
        {

            var uri = new Uri("https://github.com/dhindrik/AzServiceBus");
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            await logService.LogException(ex);
        }
    }

    [RelayCommand]
    private async Task Privacy()
    {
        try
        {

            var uri = new Uri("https://github.com/dhindrik/AzServiceBus/blob/main/PrivacyPolicy.md");
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            await logService.LogException(ex);
        }
    }
    [RelayCommand]
    private async Task DownloadLog()
    {
        try
        {
            IsBusy = true;

            var files = Directory.GetFiles(logPath);

            var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            foreach (var file in files.Where(x => x.EndsWith("log.txt")))
            {
                var text = await File.ReadAllTextAsync(file);

                var fileName = Path.GetFileName(file);

                var newPath = Path.Combine(userFolder, "Downloads", fileName);

                await File.WriteAllTextAsync(newPath, text);
            }
        }
        catch (Exception ex)
        {
            await logService.LogException(ex);

            var failedToast = Toast.Make("Downloading logs failed.");
            await failedToast.Show();
        }

        IsBusy = false;

        var toast = Toast.Make("Downloading logs completed!");
        await toast.Show();
    }

    [RelayCommand]
    private async Task OpenLicense()
    {
        try
        {
            await Browser.OpenAsync("https://www.apple.com/legal/internet-services/itunes/dev/stdeula/");
        }
        catch (Exception ex)
        {
            await logService.LogException(ex);
        }
    }
}

