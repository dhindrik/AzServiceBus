
namespace ServiceBusManager.Models;

public record QueueOrTopic
{
    public string? Name { get; init; }
    public TimeSpan? LockDuration { get; init; }
    public TimeSpan DefaultMessageTimeToLive { get; init; }
    public int? MaxDeliveryCount { get; init; }
    public long? MaxMessageSizeInKilobytes { get; init; }
    public long MaxSizeInMegabytes { get; init; }
    public EntityStatus Status { get; init; }
    public bool? RequiresSession { get; init; }
    public EntityType Type { get; set; }
}

public enum EntityType
{
    Queue,
    Topic
}

