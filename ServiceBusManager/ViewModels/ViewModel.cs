using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;

namespace ServiceBusManager.ViewModels;

public partial class ViewModel : TinyViewModel
{
    [ObservableProperty]
    private ObservableCollection<ConnectionInfo> connections = new();

    private static Dictionary<string, Action> Actions { get; } = new Dictionary<string, Action>();
    private static Dictionary<string, Action<object>> ParameterActions { get; } = new Dictionary<string, Action<object>>();

    protected static string? currentQueueOrTopic;

    protected static void HandleException(Exception ex)
    {
        Debug.Write(ex);
    }

    protected static Task HandleException(Exception ex, string message)
    {
        Debug.Write(ex);

        return Task.CompletedTask;
    }

    public void AddAction(string name, Action action)
    {
        Actions.Add(name, action);
    }

    public void AddAction(string name, Action<object> action)
    {
        ParameterActions.Add(name, action);
    }

    public void RunAction(string name)
    {
        if(Actions.ContainsKey(name))
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

