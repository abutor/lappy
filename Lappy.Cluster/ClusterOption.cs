namespace Lappy.Cluster;

public sealed class ClusterOption
{
    internal string NodeQueueName { get; set; } = $"node.{Guid.NewGuid().ToString("N")[0..8]}";
    internal List<Type> Contexts { get; init; } = [];
    internal string[] ConnectionStrings { get; private set; } = [];
    internal byte Priority { get; private set; } = 1;

    public ClusterOption() { }

    public ClusterOption WithNodeName(string nodeName)
    {
        NodeQueueName = "node." + nodeName;
        return this;
    }

    public ClusterOption UseRabbitMq(params string[] connectionString)
    {
        if (connectionString.Length == 0) throw new ArgumentException(null, nameof(connectionString));
        ConnectionStrings = connectionString;
        return this;
    }

    public ClusterOption WithContext<T>() where T : class, new()
    {
        Contexts.Add(typeof(T));
        return this;
    }
}
