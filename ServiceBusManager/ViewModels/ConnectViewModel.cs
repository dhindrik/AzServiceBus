using CommunityToolkit.Mvvm.Input;
using ServiceBusManager.Helpers;

namespace ServiceBusManager.ViewModels;

public sealed partial class ConnectViewModel : ViewModel
{
    private readonly IConnectionService connectionService;
    private readonly IFeatureService featureService;

    public ConnectViewModel(IConnectionService connectionService, IFeatureService featureService, ILogService logService) : base(logService)
    {
        this.connectionService = connectionService;
        this.featureService = featureService;
    }

    public override async Task OnAppearing()
    {
        await base.OnAppearing();

        try
        {
            IsBusy = true;

            var connectionList = await connectionService.Get();

            if (connectionList.Count == 0)
            {
                ShowNew = true;
                ShowConnections = false;

                return;
            }

            Connections = new ObservableCollection<ConnectionInfo>(connectionList);
            ShowConnections = true;
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public override async Task OnParameterSet()
    {
        await base.OnParameterSet();

        if (NavigationParameter is string command && command == "new")
        {
            ShowNew = true;
            ShowConnections = false;
        }
    }

    [ObservableProperty]
    private bool showNew;

    [ObservableProperty]
    private bool showConnections;

    [ObservableProperty]
    private string? name;

    [ObservableProperty]
    private string? connectionString;

    [ObservableProperty]
    private bool showSaveValidationMessage;

    [ObservableProperty]
    private bool showConnectValidationMessage;

    [RelayCommand]
    private void ToggleNew()
    {
        ShowNew = !ShowNew;
        ShowConnections = !ShowConnections;
    }

    [RelayCommand]
    private async Task ConnectToNew()
    {
        await OpenConnection();
    }

    [RelayCommand]
    private async Task SaveAndConnectToNew()
    {
        try
        {
            if (connectionString == SecretKeys.PremiumKey)
            {
                featureService.AddFeature(Constants.Features.Premium);
                return;
            }

            var error = false;

            if (!ValidateSave())
            {
                ShowSaveValidationMessage = true;
                error = true;
            }

            if (!ValidateConnect())
            {
                ShowConnectValidationMessage = true;
                error = true;
            }

            if(error)
            {
                return;
            }

            await connectionService.Save(new ConnectionInfo() {Id = Guid.NewGuid().ToString(), Name = name, Value = connectionString });
        }
        catch (Exception ex)
        {
            await HandleException(ex, "Could not save connection");
        }
        await OpenConnection();

        ShowNew = false;
        ShowConnections = true;

    }

    [RelayCommand]
    private async Task OpenConnection()
    {
        if (connectionString == null)
        {
            return;
        }

        if (!MainThreadHelper.IsMainThread)
        {
            MainThreadHelper.BeginInvokeOnMainThread(async () => await OpenConnection());
            return;
        }

        if (!ValidateConnect())
        {
            ShowConnectValidationMessage = true;
            return;
        }

        await Navigation.NavigateTo($"///{nameof(MainViewModel)}", connectionString);

    }

    [RelayCommand]
    private async Task ConnectToSaved(string? value)
    {
        connectionString = value;
        await OpenConnection();
    }

    [RelayCommand]
    private async Task Remove(ConnectionInfo info)
    {
        try
        {
            await connectionService.Remove(info);
            Connections.Remove(info);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private bool ValidateSave()
    {
        return !string.IsNullOrWhiteSpace(name);
    }

    private bool ValidateConnect()
    {
        return !string.IsNullOrWhiteSpace(connectionString);
    }

}

