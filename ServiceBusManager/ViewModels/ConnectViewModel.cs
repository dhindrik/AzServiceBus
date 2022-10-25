using CommunityToolkit.Mvvm.Input;

namespace ServiceBusManager.ViewModels;

public sealed partial class ConnectViewModel : ViewModel
{
    private readonly IConnectionService connectionService;

    public ConnectViewModel(IConnectionService connectionService)
    {
        this.connectionService = connectionService;
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

        if(NavigationParameter is string command && command == "new")
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
                await connectionService.Save(new ConnectionInfo() { Name = name, Value = connectionString });
            }
            catch(Exception ex)
            {
                await HandleException(ex, "Could not save connection");
            }
            await OpenConnection();
    }

    [RelayCommand]
    private async Task OpenConnection()
    {
        if (connectionString == null)
        {
            return;
        }

        if (!MainThread.IsMainThread)
        {
            MainThread.BeginInvokeOnMainThread(async () => await OpenConnection());
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

    private bool Validate()
    {
        return !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(connectionString);
    }

}

