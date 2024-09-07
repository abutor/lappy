using Castle.DynamicProxy;
using Lappy.Cluster.Acessors;
using Lappy.Cluster.Helpers;
using Lappy.Cluster.Model;
using Lappy.Cluster.Providers;
using Lappy.Core.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Lappy.Cluster;

[Service(ServiceLifetime.Scoped)]
internal class InvokeInterceptor(IMediator _mediator, IInvokeStorage _storage, ClusterAccessor _clusterAccessor) : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        var correlationId = Guid.NewGuid().ToString();
        var task = _storage.StartTask(correlationId);
        var token = (CancellationToken?)Array.Find(invocation.Arguments, x => x is CancellationToken);
        if (token.HasValue)
        {
            token.Value.Register(() => _mediator.Send(correlationId, ClusterConstants.AllNodes, new CancelRequest()));
        }

        var queueName = invocation.Method.DeclaringType!.Namespace!.ToLower();
        var queue = _storage.GetQueueForTenant(_clusterAccessor.TenantKey, queueName) ?? queueName;
        var interfaces = invocation.Method.DeclaringType.GetInterfaces().FirstOrDefault(x => x.GetCustomAttribute<RemoteServiceAttribute>() != null);
        var request = new InvokeRequest
        {
            Method = invocation.Method.Name,
            Service = invocation.Method.DeclaringType.GetFullTypeName(),
            Parameters = Array.ConvertAll(invocation.Arguments, x => SerializedModel.Create(x)),
            ParameterTypes = Array.ConvertAll(invocation.Method.GetParameters(), x => x.ParameterType.GetFullTypeName()),
            Queue = invocation.Method.DeclaringType!.Namespace!.ToLower()
        };

        _mediator.Send(correlationId, queue, request);

        try
        {
            invocation.ReturnValue = invocation.Method.ReturnType.IsAssignableTo(typeof(Task))
                ? ReflectionHelper.CastTask(task, invocation.Method.ReturnType)
                : task.Result;
        }
        catch (AggregateException ex)
        {
            if (ex.InnerException != null) throw ex.InnerException;
            else throw;
        }
    }
}
