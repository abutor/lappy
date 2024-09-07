using Lappy.Cluster.Helpers;
using Lappy.Core.Attributes;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Lappy.Cluster.Providers;

public interface IMicroservice
{
    void Start();
    void Stop();
}

[Service(ServiceLifetime.Singleton)]
internal class Microservice(IServiceProvider _provider) : IMicroservice, IDisposable
{
    private readonly ClusterOption _options = _provider.GetRequiredService<ClusterOption>();
    private readonly ClusterConsumer _consumer = _provider.GetRequiredService<ClusterConsumer>();
    private readonly HashSet<string> _queues = _provider.GetRequiredKeyedService<HashSet<string>>(ClusterConstants.QueueHashSetKey);
    private readonly IModel _consumeChannel = _provider.GetRequiredService<IConnection>().CreateModel();

    readonly List<string> _consumers = [];

    public void Start()
    {
        foreach (var queue in _queues)
        {
            _consumeChannel.QueueDeclare(queue, false, false, false);
            var consumerTag = _consumeChannel.BasicConsume(
                 _consumer,
                 queue,
                 autoAck: true,
                 arguments: new Dictionary<string, object> {
                     { "x-priority", _options.Priority },
                     { "x-expires", "60000" }
                 });
            _consumers.Add(consumerTag);
        }

        _consumeChannel.QueueDeclare(_options.NodeQueueName, false, true, true);
        var tag = _consumeChannel.BasicConsume(_consumer, _options.NodeQueueName, autoAck: true, exclusive: true);
        _consumers.Add(tag);
    }

    public void Stop()
    {
        foreach (var consumerTag in _consumers)
        {
            _consumeChannel.BasicCancel(consumerTag);
        }

        _consumeChannel.QueueDelete(_options.NodeQueueName);
        _consumers.Clear();
    }

    public void Dispose()
    {
        _consumeChannel.Dispose();
    }
}

