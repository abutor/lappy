using Lappy.Cluster;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

namespace Lappy.Core.Pool;

public static class PoolExtensions
{
    public static IServiceCollection AddPooled<TKey>(this IServiceCollection services, Func<IServiceProvider, TKey> factory)
        where TKey : class
    {
        return services.AddSingleton<ObjectPool<TKey>>(provider =>
        {
            var policy = new ServicePooledObjectPolicy<TKey>(provider, factory);
            return new DefaultObjectPool<TKey>(policy);
        }); ;
    }

    public static void UsePooled<T>(this ObjectPool<T> objectPool, Action<T> action)
        where T : class
    {
        var instance = objectPool.Get();
        try
        {
            action(instance);
        }
        finally
        {
            objectPool.Return(instance);
        }
    }

    public static TOut UsePooled<T, TOut>(this ObjectPool<T> objectPool, Func<T, TOut> action)
        where T : class
    {
        var instance = objectPool.Get();
        try
        {
            return action(instance);
        }
        finally
        {
            objectPool.Return(instance);
        }
    }
}
