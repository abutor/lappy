using RabbitMQ.Client;
using System.Reflection;
using Castle.DynamicProxy;
using Lappy.Core.Attributes;
using Lappy.Cluster.Providers;
using Microsoft.Extensions.DependencyInjection;
using Lappy.Core.Pool;
using Lappy.Cluster.Helpers;
using Lappy.Core;

namespace Lappy.Cluster;

public static class Extensions
{
    internal static string GetFullTypeName(this Type? type)
        => type == null
            ? throw new ArgumentNullException(nameof(type))
            : $"{type.FullName}, {type.Assembly.FullName}";

    internal static string GetQueueName(this Type type)
        => type.Namespace?.ToLower() ?? "undefined.namespace";

    public static IServiceCollection AddCluster(this IServiceCollection services, Action<ClusterOption>? config = null)
    {
        var options = new ClusterOption();

        config?.Invoke(options);

        var queues = new HashSet<string>();
        services.AddMemoryCache();
        services.AddServices();
        services.AddSingleton(options);
        services.AddKeyedSingleton(ClusterConstants.QueueHashSetKey, queues);
        services.AddSingleton(c => new ConnectionFactory
        {
            DispatchConsumersAsync = true,
            NetworkRecoveryInterval = TimeSpan.FromMilliseconds(100),
            RequestedHeartbeat = TimeSpan.FromSeconds(30),
            Uri = new Uri(options.RabbitUrl),
        });

        services.AddSingleton((provider) => provider.GetRequiredService<ConnectionFactory>().CreateConnection());
        services.AddScoped<IMediator, RabbitProvider>();
        services.AddPooled(provider => provider.GetRequiredService<IConnection>().CreateModel());

        var proxyGenerator = new ProxyGenerator();
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var service in assemblies.SelectMany(x => x.GetTypes()))
        {
            if (service.GetCustomAttribute<RemoteServiceAttribute>() == null) continue;

            var descriptor = services.FirstOrDefault(x => x.ServiceType == service);
            if (descriptor != null && descriptor.ImplementationType != null)
            {
                var replacement = ServiceDescriptor.DescribeKeyed(descriptor.ServiceType, ClusterConstants.InitialServiceKey, descriptor.ImplementationType, descriptor.Lifetime);
                services.Add(replacement);
                queues.Add(descriptor.ServiceType.GetQueueName());
            }

            services.AddScoped(service, provider =>
            {
                var interceptor = provider.GetRequiredService<InvokeInterceptor>();
                return proxyGenerator.CreateInterfaceProxyWithoutTarget(service, interceptor);
            });
        }

        return services;
    }
}
