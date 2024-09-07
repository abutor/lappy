using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

namespace Lappy.Cluster;

internal class ServicePooledObjectPolicy<T>(IServiceProvider _provider, Func<IServiceProvider, T> factory) : IPooledObjectPolicy<T>
    where T : class
{
    public T Create() => factory == null ? _provider.GetRequiredService<T>() : factory(_provider);
    public bool Return(T obj) => true;
}
