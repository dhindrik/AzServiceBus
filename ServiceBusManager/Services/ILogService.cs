
namespace ServiceBusManager.Services;

public interface ILogService
{
    Task LogException(Exception ex);
    Task LogPageView(string pageName, Dictionary<string, string>? properties = null);
    Task LogEvent(string eventName, Dictionary<string, string>? properties = null);
}

