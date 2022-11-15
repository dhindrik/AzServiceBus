namespace ServiceBusManager.Services;

public sealed class ServiceBusService : IServiceBusService
{
    private ServiceBusClient? client;
    private ServiceBusAdministrationClient? adminClient;
    private readonly ILogService logService;
    private readonly IConnectionService connectionService;

    private string? defaultConnectionString;

    public ServiceBusService(ILogService logService, IConnectionService connectionService)
    {
        this.logService = logService;
        this.connectionService = connectionService;
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
        if (defaultConnectionString == null)
        {
            defaultConnectionString = connectionString;
        }

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

        if (topicName != null)
        {
            var subscriptionReceiver = client.CreateReceiver(topicName, queueName);

            var subscriptionMessages = await subscriptionReceiver.PeekMessagesAsync(100);

            return subscriptionMessages.ToList();
        }

        var receiver = client.CreateReceiver(queueName);

        var messages = await receiver.PeekMessagesAsync(10000);

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

        var messages = await receiver.PeekMessagesAsync(10000);

        return messages.ToList();
    }

    public async Task AddToDeadLetter(string queueName, ServiceBusReceivedMessage message, string? topicName = null)
    {
        if (client == null)
        {
            throw new Exception("You must run init first");
        }

        ServiceBusReceiver receiver;

        if (topicName != null)
        {
            receiver = client.CreateReceiver(topicName, queueName, new ServiceBusReceiverOptions()
            {
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            });
        }
        else
        {
            receiver = client.CreateReceiver(queueName, new ServiceBusReceiverOptions()
            {
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            });
        }

        var messages = await receiver.ReceiveMessagesAsync(10000);

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

    public async Task Resend(string queueName, ServiceBusReceivedMessage message, string? editedBody = null, string? topicName = null)
    {
        try
        {
            if (client == null)
            {
                throw new Exception("You must run init first");
            }

            ServiceBusReceiver receiver;
            ServiceBusSender sender;

            if (topicName != null)
            {
                receiver = client.CreateReceiver(topicName, queueName, new ServiceBusReceiverOptions()
                {
                    SubQueue = SubQueue.DeadLetter,
                    ReceiveMode = ServiceBusReceiveMode.PeekLock
                });

                sender = client.CreateSender(topicName);
            }
            else
            {
                receiver = client.CreateReceiver(queueName, new ServiceBusReceiverOptions()
                {
                    SubQueue = SubQueue.DeadLetter,
                    ReceiveMode = ServiceBusReceiveMode.PeekLock
                });

                sender = client.CreateSender(queueName);
            }

            var messages = await receiver.ReceiveMessagesAsync(10000);

            BinaryData body;

            if (editedBody == null)
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

    public async Task Remove(string queueName, bool isDeadLetter, ServiceBusReceivedMessage message, string? topicName = null)
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

        ServiceBusReceiver receiver;
        ServiceBusSender sender;

        if (topicName != null)
        {
            receiver = client.CreateReceiver(topicName, queueName, options);

            sender = client.CreateSender(topicName);
        }
        else
        {
            receiver = client.CreateReceiver(queueName, options);

            sender = client.CreateSender(queueName);
        }

        var messages = await receiver.ReceiveMessagesAsync(10000);


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

    public async Task<int> CheckNewDeadLetters(DateTimeOffset dateTime)
    {
        var connections = await connectionService.Get();

        int count = 0;

        foreach (var connection in connections)
        {
            if (connection.Value == null)
            {
                continue;
            }

            try
            {
                await Init(connection.Value);

                var queues = await GetQueues();

                foreach (var queue in queues)
                {
                    if (queue == null || queue.Name == null)
                    {
                        continue;
                    }

                    var deadLetters = await PeekDeadLetter(queue.Name);
                    count += deadLetters.Where(x => x.EnqueuedTime > dateTime).Count();
                }

                var topics = await GetTopics();

                foreach (var topic in topics)
                {
                    if (topic == null || topic.Name == null)
                    {
                        continue;
                    }

                    var subscriptions = await GetSubscriptions(topic.Name);

                    foreach (var queue in subscriptions)
                    {
                        if (queue == null || queue.Name == null)
                        {
                            continue;
                        }

                        var deadLetters = await PeekDeadLetter(queue.Name, topic.Name);
                        count += deadLetters.Where(x => x.EnqueuedTime > dateTime).Count();
                    }
                }
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () => await logService.LogException(ex));
            }
        }

        if (defaultConnectionString != null)
        {
            await Init(defaultConnectionString);
        }

        return count;
    }

    public async Task<Dictionary<string, List<(string Name, int Count)>>> GetDeadLetters()
    {
        var connections = await connectionService.Get();

        Dictionary<string, List<(string Name, int Count)>> deadLetterCounts = new();

        foreach (var connection in connections)
        {

            if (connection.Value == null || connection.Name == null)
            {
                continue;
            }

            try
            {
                await Init(connection.Value);

                var queues = await GetQueues();

                foreach (var queue in queues)
                {
                    if (queue == null || queue.Name == null)
                    {
                        continue;
                    }

                    var deadLetters = await PeekDeadLetter(queue.Name);

                    if (deadLetters.Count > 0)
                    {
                        if (!deadLetterCounts.ContainsKey(connection.Name))
                        {
                            deadLetterCounts.Add(connection.Name, new List<(string Name, int Count)>());
                        }

                        deadLetterCounts[connection.Name].Add((queue.Name, deadLetters.Count));
                    }
                }

                var topics = await GetTopics();

                foreach (var topic in topics)
                {
                    if (topic == null || topic.Name == null)
                    {
                        continue;
                    }

                    var subscriptions = await GetSubscriptions(topic.Name);

                    foreach (var queue in subscriptions)
                    {
                        if (queue == null || queue.Name == null)
                        {
                            continue;
                        }

                        var deadLetters = await PeekDeadLetter(queue.Name, topic.Name);

                        if (deadLetters.Count > 0)
                        {
                            if (!deadLetterCounts.ContainsKey(connection.Name))
                            {
                                deadLetterCounts.Add(connection.Name, new List<(string Name, int Count)>());
                            }

                            deadLetterCounts[connection.Name].Add(($"{topic.Name}/{queue.Name}", deadLetters.Count));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () => await logService.LogException(ex));
            }
        }
        return deadLetterCounts;
    }
}
