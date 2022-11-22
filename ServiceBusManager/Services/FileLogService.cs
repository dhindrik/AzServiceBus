namespace ServiceBusManager.Services;

public class FileLogService : ILogService
{
    public FileLogService()
    {
    }

    public async Task LogEvent(string eventName, Dictionary<string, string>? properties = null)
    {
        await Write($"EventName: {eventName}");
    }

    public async Task LogException(Exception ex)
    {
        await Write($"ExceptionMessage: {ex.Message}", $"StackTrace: {ex.StackTrace}");
    }

    public async Task LogPageView(string pageName)
    {
        await Write($"PageName: {pageName}");
    }

    private async Task Write(params string[] text)
    {
#if DEBUG
        var path = Path.Combine(FileSystem.AppDataDirectory, "AzServiceBus-DEBUG", "logs");
#else
        var path = Path.Combine(FileSystem.AppDataDirectory, "logs");
#endif

        Directory.CreateDirectory(path);

        var filePath = Path.Combine(path, $"{DateTime.Now.ToShortDateString()}-log.txt");

        var lines = new List<string>()
        {
            $"---{DateTime.Now.ToString()}---",

        };

        lines.AddRange(text);
        lines.Add("");

        await File.AppendAllLinesAsync(filePath, lines);
    }
}

