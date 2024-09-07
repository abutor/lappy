using Microsoft.Extensions.DependencyInjection;

namespace Lappy.Cluster.Acessors;

[Service(ServiceLifetime.Scoped)]
public class ClusterAccessor
{
    public string TenantKey { get; set; } = string.Empty;
    public bool IsReadOperation { get; set; }
}
