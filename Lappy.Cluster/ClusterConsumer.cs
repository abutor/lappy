using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Lappy.Cluster.Providers;
using Lappy.Cluster.Acessors;
using Lappy.Cluster.Helpers;
using Newtonsoft.Json;
using System.Reflection;
using Lappy.Cluster.Model;

namespace Lappy.Cluster;

[Service(ServiceLifetime.Singleton)]
internal class ClusterConsumer(IServiceProvider _provider) : AsyncDefaultBasicConsumer
{
    public override async Task HandleBasicDeliver(
        string consumerTag,
        ulong deliveryTag,
        bool redelivered,
        string exchange,
        string routingKey,
        IBasicProperties properties,
        ReadOnlyMemory<byte> body)
    {
        using var scope = _provider.CreateScope();
        var rabbit = scope.ServiceProvider.GetRequiredService<IMediator>();
        try
        {
            var accessor = scope.ServiceProvider.GetRequiredService<RequestAccessor>();
            var bodies = BodySerializer.Deserialize(body.ToArray());
            if (bodies != null)
            {
                accessor.SetupRequest(properties, bodies);
            }

            LockTenantKey(scope.ServiceProvider, routingKey, accessor.TenantKey);
            await ReflectionHelper.Handle(scope.ServiceProvider, accessor.CorrelationId, accessor.Body);
        }
        catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
        {
            rabbit.Send(properties.CorrelationId, properties.ReplyTo, new CancelRequest());
        }
        catch (Exception ex)
        {
            if (ex is TargetInvocationException target && target.InnerException != null) ex = target.InnerException;

            var json = JsonConvert.SerializeObject(ex);
            rabbit.Send(properties.CorrelationId, properties.ReplyTo, new ExceptionRequest(json, ex.GetType().GetFullTypeName()));
        }
    }

    private static void LockTenantKey(IServiceProvider provider, string routingKey, string? tenantId)
    {
        if (string.IsNullOrEmpty(tenantId)) return;

        var options = provider.GetRequiredService<ClusterOption>();
        var rabbit = provider.GetRequiredService<IMediator>();
        rabbit.Send(Guid.NewGuid().ToString(), ClusterConstants.AllNodes, new LockTenantRequest(tenantId, routingKey, options.NodeQueueName));
    }
}
