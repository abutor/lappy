using Microsoft.Extensions.DependencyInjection;

namespace Lappy.Cluster.Test.Helpers
{
    [Service(ServiceLifetime.Scoped)]
    public class TestRequestContext
    {
        public int RequestId { get; set; }
    }
}
