namespace ServiceBusManager.Services;

public interface IConnectionService
{
    Task<List<ConnectionInfo>> Get();
    Task Save(ConnectionInfo connection);
    Task Remove(ConnectionInfo connection);
}
