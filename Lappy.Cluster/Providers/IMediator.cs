using Lappy.Cluster.Acessors;
using Lappy.Cluster.Helpers;
using Lappy.Core.Attributes;
using Lappy.Core.Pool;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;

namespace Lappy.Cluster.Providers;

public interface IMediator
{
    void Send<T>(string correlationId, string queue, T body);
}

[Service(ServiceLifetime.Scoped)]
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
            properties.Priority = _clusterOption.Priority;
            properties.ReplyTo = _clusterOption.NodeQueueName;
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
            bodies.Add(new BodyItem(type.GetFullTypeName(), context));
        }

        return BodySerializer.Serialize([.. bodies]);
    }
}
