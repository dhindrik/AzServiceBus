namespace ServiceBusManager.Models;

public record DeadLetterInfo
{
    public DeadLetterInfo(string name, int count, string? topic = null)
    {
        Name = name;
        Count = count;
        Topic = topic;
    }

    public string Name { get; set; }
    public int Count { get; set; }
    public string? Topic { get; set; }
    public string Fullname
    {
        get
        {
            if(Topic != null)
            {
                return $"{Topic}/{Name}";
            }

            return Name;
        }
    }

    public string Connection { get; set; }
}

