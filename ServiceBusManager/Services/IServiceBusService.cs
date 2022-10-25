namespace ServiceBusManager.Services;

public interface IServiceBusService
{
    Task Init(string connectionString);
    Task<List<QueueOrTopic>> GetQueues();
    Task<QueueOrTopic> GetQueue(string name);
    Task<List<ServiceBusReceivedMessage>> Peek(string queueName);
    Task<List<ServiceBusReceivedMessage>> PeekDeadLetter(string queueName);
    Task AddToDeadLetter(string queueName, ServiceBusReceivedMessage message);
    Task Resend(string queueName, ServiceBusReceivedMessage message);
    Task<string> GetNamepspace();
    Task Remove(string queueName, bool isDeadLetter, ServiceBusReceivedMessage message);

    Task<List<QueueOrTopic>> GetTopics();
    Task<List<Subscription>> GetSubscriptions(string topicName);
}
