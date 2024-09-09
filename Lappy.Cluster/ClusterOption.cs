using System;
using System.Reflection;

namespace Lappy.Cluster;

public sealed class ClusterOption
{
    internal string NodeQueueName { get; set; } = $"node.{Guid.NewGuid().ToString("N")[0..8]}";
    internal List<Type> Contexts { get; init; } = [];
    internal string[] RabbitHosts { get; private set; } = [];
    internal string RabbitUrl { get; set; } = string.Empty;
    internal bool IsSingleNode { get; set; } = true;
    internal Assembly ParentAssembly { get; set; }

    internal string Username { get; set; }
    internal string Password { get; set; }
    internal string VHost { get; set; }
    internal int Port { get; set; }
    internal byte Priority { get; private set; } = 1;

    public ClusterOption() { }

    public ClusterOption WithNodeName(string nodeName)
    {
        NodeQueueName = "node." + nodeName;
        return this;
    }

    public ClusterOption WithAssembly(Assembly assembly)
    {
        ParentAssembly = assembly;
        return this;
    }

    //public ClusterOption UseRabbitMq(string username, string password, int port, string vHost, string[] hostnames)
    //{
    //    Username = username;
    //    Password = password;
    //    Port = port;
    //    VHost = vHost;
    //    RabbitHosts = hostnames;

    //    IsSingleNode = false;
    //    return this;
    //}

    public ClusterOption UseRabbitMqHosts(string url)
    {
        RabbitUrl = url;
        IsSingleNode = true;
        return this;
    }

    public ClusterOption WithContext<T>() where T : class, new()
    {
        Contexts.Add(typeof(T));
        return this;
    }
}
