using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;

namespace ServiceBusManager.ViewModels;

public partial class ViewModel : TinyViewModel
{
    private readonly ILogService logService;

    private static Dictionary<string, Action> Actions { get; } = new Dictionary<string, Action>();
    private static Dictionary<string, Action<object>> ParameterActions { get; } = new Dictionary<string, Action<object>>();

    protected static string? currentQueueOrTopic;

    public ViewModel()
    {
        var log = Resolver.Resolve<ILogService>();

        if (log == null)
        {
            throw new NullReferenceException("ILogService need to be added to IoC container");
        }

        logService = log;
    }

    [ObservableProperty]
    private ObservableCollection<ConnectionInfo> connections = new();

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

