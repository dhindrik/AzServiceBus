namespace ServiceBusManager.Models
{
    public class CollectionGroup<T> : List<T>
    {
        public CollectionGroup(string name, List<T> items) : base(items)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}

