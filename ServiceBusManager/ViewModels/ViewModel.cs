using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;

namespace ServiceBusManager.ViewModels;

public partial class ViewModel : TinyViewModel
{
    private readonly ILogService logService;
    private IPremiumService? premiumService;

    private static Dictionary<string, Action> Actions { get; } = new Dictionary<string, Action>();
    private static Dictionary<string, Action<object>> ParameterActions { get; } = new Dictionary<string, Action<object>>();

    protected static string? currentQueueOrTopic;
    protected static bool? hasPremium;

    public ViewModel(ILogService logService)
    {
        this.logService = logService;
    }

    public override async Task Initialize()
    {
        await base.Initialize();

        var service = Resolver.Resolve<IPremiumService>();
        if (service == null)
        {
            throw new NullReferenceException("IFeatureService need to be added to IoC container");
        }

        premiumService = service;

        service.PremiumChanged += (sender, args) => CheckPremium();

        if (!hasPremium.HasValue)
        {
            CheckPremium();
        }
    }

    private void CheckPremium()
    {
        _ = Task.Run(() =>
        {
            hasPremium = premiumService?.HasPremium();

            MainThread.BeginInvokeOnMainThread(() => {
                OnPropertyChanged(nameof(HasPremium));
                OnPropertyChanged(nameof(HasNotPremium));
            });
        });
    }

    [ObservableProperty]
    private ObservableCollection<ConnectionInfo> connections = new();

    public bool HasPremium => hasPremium.HasValue ? hasPremium.Value : false;
    public bool HasNotPremium => !HasPremium;

    protected void HandleException(Exception ex)
    {
        if (logService != null)
        {
            _ = Task.Run(async () => await logService.LogException(ex));
        }
    }

    protected Task HandleException(Exception ex, string message)
    {
        if (logService != null)
        {
            _ = Task.Run(async () => await logService.LogException(ex));
        }

        return Task.CompletedTask;
    }

    public void AddAction(string name, Action action)
    {
        if (Actions.ContainsKey(name))
        {
            Actions[name] = action;
            return;
        }

        Actions.Add(name, action);
    }

    public void AddAction(string name, Action<object> action)
    {
        if (ParameterActions.ContainsKey(name))
        {
            ParameterActions[name] = action;
            return;
        }

        ParameterActions.Add(name, action);
    }

    public void RunAction(string name)
    {
        if (Actions.ContainsKey(name))
        {
            Actions[name].Invoke();
        }
    }

    public void RunAction(string name, object parameter)
    {
        if (ParameterActions.ContainsKey(name))
        {
            ParameterActions[name].Invoke(parameter);
        }
    }

    public void RemoveAction(string name)
    {
        ParameterActions.Remove(name);
    }

    [RelayCommand]
    private void Menu()
    {
        Shell.Current.FlyoutIsPresented = true;
    }
}

