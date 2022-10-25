namespace ServiceBusManager.Selectors;

public sealed class QueueOrTopicSelector : DataTemplateSelector
{
    public DataTemplate? QueueTemplate { get; set; }
    public DataTemplate? TopicTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (item is QueueOrTopic queueOrTopic)
        {
            if (queueOrTopic.Type == EntityType.Queue && QueueTemplate != null)
            {
                return QueueTemplate;
            }
            else if (TopicTemplate != null)
            {
                return TopicTemplate;
            }

            throw new NullReferenceException("QueueTemplate and/or TopicTemplate is null");
        }

        return null!;
    }
}

