using RabbitMQ.Client;
using Lappy.Cluster.Acessors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

namespace Lappy.Cluster.Providers;

internal class RabbitProvider(IServiceProvider _provider) : IMediator
{
    private readonly ClusterOption _clusterOption = _provider.GetRequiredService<ClusterOption>();
    private readonly ClusterAccessor _clusterAccessor = _provider.GetRequiredService<ClusterAccessor>();
    private readonly ObjectPool<IModel> _channelPools = _provider.GetRequiredService<ObjectPool<IModel>>();

    public void Send<T>(string correlationId, string queueName, T body)
    {
        var request = GetBody(body);

        _channelPools.UsePooled(channel =>
        {
            var properties = channel.CreateBasicProperties();
            properties.CorrelationId = correlationId;
            properties.ReplyTo = _clusterOption.NodeQueueName;
            properties.Priority = _clusterOption.Priority;
            channel.BasicPublish(string.Empty, queueName, properties, request);
        });
    }

    private byte[] GetBody<T>(T body)
    {
        var bodies = new List<BodyItem> { new(ClusterConstants.RequestBodyKey, body) };

        if (!_clusterAccessor.IsReadOperation && !string.IsNullOrEmpty(_clusterAccessor.TenantKey))
        {
            bodies.Add(new BodyItem(ClusterConstants.RequestTenantKey, _clusterAccessor.TenantKey));
        }

        foreach (var type in _clusterOption.Contexts)
        {
            var context = _provider.GetRequiredService(type);
            bodies.Add(new BodyItem(type.FullName!, context));
        }

        return BodySerializer.Serialize([.. bodies]);
    }
}
