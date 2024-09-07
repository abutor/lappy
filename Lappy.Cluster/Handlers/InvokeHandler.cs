using Lappy.Cluster.Acessors;
using Lappy.Cluster.Helpers;
using Lappy.Cluster.Model;
using Lappy.Cluster.Providers;
using Lappy.Core.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Lappy.Cluster.Handlers;

[Service(ServiceLifetime.Scoped)]
internal class InvokeHandler(IServiceProvider _provider) : IHandler<InvokeRequest>
{
    public async Task Handle(string correlationId, InvokeRequest request)
    {
        var invokeProvider = _provider.GetRequiredService<IMediator>();
        var requestAccessor = _provider.GetRequiredService<RequestAccessor>();
        var serviceType = Type.GetType(request.Service)
            ?? throw new ClusterException($"{request.Service} type is undefined");

        var method = serviceType.GetMethod(request.Method, Array.ConvertAll(request.ParameterTypes, Type.GetType)!)
            ?? throw new ClusterException($"{request.Service} type has not method {request.Method}");

        var service = _provider.GetRequiredKeyedService(serviceType, ClusterConstants.InitialServiceKey)
            ?? throw new ClusterException($"{request.Service} is not registred");

        var storage = _provider.GetRequiredService<IInvokeStorage>();

        var task = storage.CreateCancel(correlationId);

        var parameters = Array.ConvertAll(request.Parameters, x => x?.GetObject());
        var cancelIndex = Array.FindIndex(request.Parameters, x => x?.GetObjectType() == typeof(CancellationToken));
        if (cancelIndex >= 0)
        {
            var cancel = storage.CreateCancel(correlationId);
            cancel.Register(() => invokeProvider.Send(correlationId, requestAccessor.ReplyTo!, new CancelRequest()));
            parameters[cancelIndex] = cancel;
        }

        var invokeResult = method.Invoke(service, parameters);
        var model = await PrepareResult(invokeResult, method);

        invokeProvider.Send(correlationId, requestAccessor.ReplyTo!, new ReplyRequest(model));
    }

    private async static ValueTask<SerializedModel?> PrepareResult(object? result, MethodInfo method)
    {
        var returnType = method.ReturnType;
        if (result is Task taskResult)
        {
            await taskResult;
            if (method.ReturnType != typeof(Task))
            {
                result = result?.GetType().GetProperty("Result")?.GetValue(taskResult);
                returnType = method.ReturnType.GenericTypeArguments.FirstOrDefault();
            }
            else
            {
                result = null;
                returnType = typeof(object);
            }
        }

        return SerializedModel.Create(result, returnType);
    }
}
