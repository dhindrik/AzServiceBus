namespace ServiceBusManager.Services;

public interface IServiceBusService
{
    Task Init(string connectionString);
    Task<List<QueueOrTopic>> GetQueues();
    Task<QueueOrTopic> GetQueue(string name);
    Task<List<ServiceBusReceivedMessage>> Peek(string queueName, string? topicName = null);
    Task<List<ServiceBusReceivedMessage>> PeekDeadLetter(string queueName, string? topicName = null);
    Task AddToDeadLetter(string queueName, ServiceBusReceivedMessage message);
    Task Resend(string queueName, ServiceBusReceivedMessage message, string? editedBody = null);
    Task<string> GetNamepspace();
    Task Remove(string queueName, bool isDeadLetter, ServiceBusReceivedMessage message);

    Task<List<QueueOrTopic>> GetTopics();
    Task<List<Subscription>> GetSubscriptions(string topicName);
}
