using System;
using System.Diagnostics;

namespace ServiceBusManager.Services;

public sealed class DebugConnectionService : IConnectionService
	{
    private const string storageKey = "connections.txt";


    public async Task<List<ConnectionInfo>> Get()
    {
        var path = GetAndEnsurePath();

        var json = await File.ReadAllTextAsync(path);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<ConnectionInfo>();
        }

        var connections = JsonSerializer.Deserialize<List<ConnectionInfo>>(json, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });

        return connections ?? new List<ConnectionInfo>();
    }

    private string GetAndEnsurePath()
    {
        var path = Path.Combine(FileSystem.Current.CacheDirectory, storageKey);

        if(!File.Exists(path))
        {
            File.Create(path);
        }

        return path;
    }

    public async Task Remove(ConnectionInfo connection)
    {
        var connections = await Get();

        var current = connections.Single(x => x.Id == connection.Id);

        connections.Remove(current);

        var json = JsonSerializer.Serialize(connections);

        var path = GetAndEnsurePath();
        await File.WriteAllTextAsync(path, json);
    }

    public async Task Save(ConnectionInfo connection)
    {
        var connections = await Get();

        connections.Add(connection);

        var json = JsonSerializer.Serialize(connections);

        var path = GetAndEnsurePath();

        await File.WriteAllTextAsync(path, json);
    }
}

