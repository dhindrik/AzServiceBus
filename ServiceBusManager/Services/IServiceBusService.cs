namespace ServiceBusManager.Services;

public interface IServiceBusService
{
    Task Init(string connectionString);
    Task<List<QueueOrTopic>> GetQueues();
    Task<QueueOrTopic> GetQueue(string name);
    Task<QueueOrTopic> GetTopic(string name);
    Task<List<ServiceBusReceivedMessage>> Peek(string queueName, string? topicName = null);
    Task<List<ServiceBusReceivedMessage>> PeekDeadLetter(string queueName, string? topicName = null);
    Task AddToDeadLetter(string queueName, ServiceBusReceivedMessage message, string? topicName = null);
    Task Resend(string queueName, ServiceBusReceivedMessage message, string? editedBody = null, string? topicName = null);
    Task Send(string queueName, ServiceBusMessage message, string? topicName = null);
    Task<string> GetNamepspace();
    Task Remove(string queueName, bool isDeadLetter, ServiceBusReceivedMessage message, string? topicName = null);

    Task<List<QueueOrTopic>> GetTopics();
    Task<List<Subscription>> GetSubscriptions(string topicName);

    Task<int> CheckNewDeadLetters(DateTimeOffset dateTime);
    Task<Dictionary<string, List<(string Name, int Count)>>> GetDeadLetters();
}
