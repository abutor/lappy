using Lappy.Cluster.Helpers;
using Lappy.Cluster.Model;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System.Reflection;

namespace Lappy.Cluster.Acessors;

[Service(ServiceLifetime.Scoped)]
internal class RequestAccessor(ClusterOption _option, IServiceProvider _provider)
{
    private ICollection<BodyItem> _serializedModel = [];

    public string CorrelationId { get; private set; } = string.Empty;
    public string ReplyTo { get; private set; } = string.Empty;
    public string TenantKey { get; private set; } = string.Empty;

    public object? Body { get; private set; } = null;

    public void SetupRequest(IBasicProperties properties, ICollection<BodyItem> data)
    {
        CorrelationId = properties.CorrelationId;
        ReplyTo = properties.ReplyTo;
        _serializedModel = data ?? throw new ClusterException("Body deserialize with error");

        Body = _serializedModel.FirstOrDefault(x => x.Key == ClusterConstants.RequestBodyKey)?.Value ?? throw new ClusterException("Body was not found");
        TenantKey = _serializedModel.FirstOrDefault(x => x.Key == ClusterConstants.RequestTenantKey)?.Value?.ToString() ?? string.Empty;

        SetupContext([.. _serializedModel]);
    }

    private void SetupContext(BodyItem[] contexts)
    {
        foreach (var type in _option.Contexts)
        {
            var requestContext = contexts.FirstOrDefault(x => x.Key == type.GetFullTypeName())?.Value;
            if (requestContext == null) continue;

            var scopeContext = _provider.GetRequiredService(type);
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = property.GetValue(requestContext, null);
                property.SetValue(scopeContext, value);
            }
        }
    }
}
