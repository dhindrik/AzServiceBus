namespace ServiceBusManager.Models;

public record Subscription
{
    public string? Name { get; init; }
    public EntityStatus Status { get; init; }

}

