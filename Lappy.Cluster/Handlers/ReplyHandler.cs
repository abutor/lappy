using Lappy.Cluster.Model;
using Lappy.Cluster.Providers;
using Lappy.Core.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Lappy.Cluster.Handlers;

[Service(ServiceLifetime.Scoped)]
internal class ReplyHandler(IInvokeStorage _storage) : IHandler<ReplyRequest>, IHandler<ExceptionRequest>, IHandler<CancelRequest>, IHandler<LockTenantRequest>
{
    public Task Handle(string correlationId, ReplyRequest request)
    {
        _storage.Complete(correlationId, request.Body?.GetObject());
        return Task.CompletedTask;
    }

    public Task Handle(string correlationId, CancelRequest request)
    {
        _storage.Cancel(correlationId);
        return Task.CompletedTask;
    }

    public Task Handle(string correlationId, LockTenantRequest request)
    {
        _storage.LockTenant(request.TenantKey, request.ServiceQueue, request.NodeQueue);
        return Task.CompletedTask;
    }

    public Task Handle(string correlationId, ExceptionRequest request)
    {
        var exceptionType = Type.GetType(request.ExceptionType);
        if (exceptionType == null)
        {
            _storage.Failed(correlationId, new Exception());
            return Task.CompletedTask;
        }

        var result = (Exception?)JsonConvert.DeserializeObject(request.JsonString, exceptionType);
        if (result != null)
        {
            _storage.Failed(correlationId, result);
        }

        return Task.CompletedTask;
    }
}
