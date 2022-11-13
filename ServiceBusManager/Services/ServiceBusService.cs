namespace ServiceBusManager.Services;

public sealed class ServiceBusService : IServiceBusService
{
    private ServiceBusClient? client;
    private ServiceBusAdministrationClient? adminClient;
    private readonly ILogService logService;

    public ServiceBusService(ILogService logService)
    {
        this.logService = logService;
    }

    public async Task<QueueOrTopic> GetQueue(string name)
    {
        if (adminClient == null)
        {
            throw new Exception("You must run init first");
        }

        var queue = await adminClient.GetQueueAsync(name);


        var result = new QueueOrTopic()
        {
            Name = queue.Value.Name,
            DefaultMessageTimeToLive = queue.Value.DefaultMessageTimeToLive,
            LockDuration = queue.Value.LockDuration,
            MaxDeliveryCount = queue.Value.MaxDeliveryCount,
            MaxMessageSizeInKilobytes = queue.Value.MaxMessageSizeInKilobytes,
            MaxSizeInMegabytes = queue.Value.MaxSizeInMegabytes,
            Status = queue.Value.Status,
            RequiresSession = queue.Value.RequiresSession
        };


        return result;
    }

    public async Task<List<QueueOrTopic>> GetQueues()
    {
        if (adminClient == null)
        {
            throw new Exception("You must run init first");
        }

        var queues = adminClient.GetQueuesAsync();
        var enumerator = queues.GetAsyncEnumerator();

        var queueNames = new List<QueueOrTopic>();

        while (await enumerator.MoveNextAsync())
        {
            if (enumerator.Current == null || queueNames.Any(x => x.Name == enumerator.Current.Name))
            {
                break;
            }

            queueNames.Add(new QueueOrTopic()
            {
                Name = enumerator.Current.Name,
                DefaultMessageTimeToLive = enumerator.Current.DefaultMessageTimeToLive,
                LockDuration = enumerator.Current.LockDuration,
                MaxDeliveryCount = enumerator.Current.MaxDeliveryCount,
                MaxMessageSizeInKilobytes = enumerator.Current.MaxMessageSizeInKilobytes,
                MaxSizeInMegabytes = enumerator.Current.MaxSizeInMegabytes,
                Status = enumerator.Current.Status,
                RequiresSession = enumerator.Current.RequiresSession,
                Type = EntityType.Queue
            });
        }

        return queueNames;
    }

    public async Task<List<QueueOrTopic>> GetTopics()
    {
        if (adminClient == null)
        {
            throw new Exception("You must run init first");
        }

        var queues = adminClient.GetTopicsAsync();
        var enumerator = queues.GetAsyncEnumerator();

        var topicNames = new List<QueueOrTopic>();

        while (await enumerator.MoveNextAsync())
        {

            if (enumerator.Current == null || topicNames.Any(x => x.Name == enumerator.Current.Name))
            {
                break;
            }

            topicNames.Add(new QueueOrTopic()
            {
                Name = enumerator.Current.Name,
                DefaultMessageTimeToLive = enumerator.Current.DefaultMessageTimeToLive,
                MaxMessageSizeInKilobytes = enumerator.Current.MaxMessageSizeInKilobytes,
                MaxSizeInMegabytes = enumerator.Current.MaxSizeInMegabytes,
                Status = enumerator.Current.Status,
                Type = EntityType.Topic
            });
        }

        return topicNames;
    }

    public Task Init(string connectionString)
    {
        client = new ServiceBusClient(connectionString);
        adminClient = new ServiceBusAdministrationClient(connectionString);

        return Task.CompletedTask;
    }

    public async Task<List<ServiceBusReceivedMessage>> Peek(string queueName, string? topicName = null)
    {
        if (client == null)
        {
            throw new Exception("You must run init first");
        }

        if(topicName != null)
        {
            var subscriptionReceiver = client.CreateReceiver(topicName, queueName);

            var subscriptionMessages = await subscriptionReceiver.PeekMessagesAsync(100);

            return subscriptionMessages.ToList();
        }

        var receiver = client.CreateReceiver(queueName);

        var messages = await receiver.PeekMessagesAsync(100);

        return messages.ToList();
    }

    public async Task<List<ServiceBusReceivedMessage>> PeekDeadLetter(string queueName, string? topicName = null)
    {
        if (client == null)
        {
            throw new Exception("You must run init first");
        }

        if (topicName != null)
        {
            var subscriptionReceiver = client.CreateReceiver(topicName, queueName, new ServiceBusReceiverOptions()
            {
                SubQueue = SubQueue.DeadLetter
            });

            var subscriptionMessages = await subscriptionReceiver.PeekMessagesAsync(100);

            return subscriptionMessages.ToList();
        }

        var receiver = client.CreateReceiver(queueName, new ServiceBusReceiverOptions()
        {
            SubQueue = SubQueue.DeadLetter
        });

        var messages = await receiver.PeekMessagesAsync(100);

        return messages.ToList();
    }

    public async Task AddToDeadLetter(string queueName, ServiceBusReceivedMessage message)
    {
        if (client == null)
        {
            throw new Exception("You must run init first");
        }

        var receiver = client.CreateReceiver(queueName, new ServiceBusReceiverOptions()
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock
        });

        var messages = await receiver.ReceiveMessagesAsync(100);

        var messageToAdd = messages.Single(x => x.MessageId == message.MessageId && x.EnqueuedTime == message.EnqueuedTime);

        await receiver.DeadLetterMessageAsync(messageToAdd);

        foreach (var msg in messages)
        {
            if (msg == messageToAdd)
            {
                continue;
            }

            await receiver.AbandonMessageAsync(msg);
        }

        await receiver.DisposeAsync();
    }

    public async Task Resend(string queueName, ServiceBusReceivedMessage message, string? editedBody = null)
    {
        try
        {
            if (client == null)
            {
                throw new Exception("You must run init first");
            }

            var receiver = client.CreateReceiver(queueName, new ServiceBusReceiverOptions()
            {
                SubQueue = SubQueue.DeadLetter,
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            });

            var messages = await receiver.ReceiveMessagesAsync(100);

            var sender = client.CreateSender(queueName);

            BinaryData body;

            if(editedBody == null)
            {
                body = message.Body;
            }
            else
            {
                body = BinaryData.FromString(editedBody);
            }

            var messageToAdd = new ServiceBusMessage()
            {
                SessionId = message.SessionId,
                Body = body
            };

            await sender.SendMessageAsync(messageToAdd);

            var oldMessage = messages.Single(x => x.MessageId == message.MessageId && x.EnqueuedTime == message.EnqueuedTime);

            await receiver.CompleteMessageAsync(oldMessage);
        }
        catch (Exception)
        {

        }
    }

    public Task<string> GetNamepspace()
    {
        if (client == null)
        {
            throw new Exception("You must run init first");
        }

        return Task.FromResult(client.FullyQualifiedNamespace);
    }

    public async Task Remove(string queueName, bool isDeadLetter, ServiceBusReceivedMessage message)
    {

        if (client == null)
        {
            throw new Exception("You must run init first");
        }

        var options = new ServiceBusReceiverOptions()
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock
        };


        if (isDeadLetter)
        {
            options.SubQueue = SubQueue.DeadLetter;
        }

        var receiver = client.CreateReceiver(queueName, options);

        var messages = await receiver.ReceiveMessagesAsync(100);

        var sender = client.CreateSender(queueName);

        var oldMessage = messages.Single(x => x.MessageId == message.MessageId && x.EnqueuedTime == message.EnqueuedTime);

        await receiver.CompleteMessageAsync(oldMessage);
    }

    public async Task<List<Subscription>> GetSubscriptions(string topicName)
    {
        if (adminClient == null)
        {
            throw new Exception("You must run init first");
        }

        var subscriptions = adminClient.GetSubscriptionsAsync(topicName);
        var enumerator = subscriptions.GetAsyncEnumerator();

        var subscriptionNames = new List<Subscription>();

        while (await enumerator.MoveNextAsync())
        {          

            if (enumerator.Current == null || subscriptionNames.Any(x => x.Name == enumerator.Current.SubscriptionName))
            {
                break;
            }

            subscriptionNames.Add(new Subscription()
            {
                Name = enumerator.Current.SubscriptionName,
                Status = enumerator.Current.Status
            });
        }

        return subscriptionNames;
    }
}
