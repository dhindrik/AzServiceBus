namespace ServiceBusManager.Services;

public sealed class SecureConnectionService : IConnectionService
{
    private const string storageKey = "connections";

    public SecureConnectionService()
    {
    }

    public async Task<List<ConnectionInfo>> Get()
    {
        var json = await SecureStorage.GetAsync(storageKey);

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

    public async Task Remove(ConnectionInfo connection)
    {
        var connections = await Get();

        var current = connections.Single(x => x.Value == connection.Value);

        connections.Remove(current);

        var json = JsonSerializer.Serialize(connections);

        await SecureStorage.SetAsync(storageKey, json);
    }

    public async Task Save(ConnectionInfo connection)
    {
        var connections = await Get();

        connections.Add(connection);

        var json = JsonSerializer.Serialize(connections);

        await SecureStorage.SetAsync(storageKey, json);
    }
}
