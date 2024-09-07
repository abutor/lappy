using Lappy.Cluster.Model;
using Lappy.Core.Attributes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Lappy.Cluster.Providers;

public interface IInvokeStorage
{
    CancellationToken CreateCancel(string key);
    Task<object?> StartTask(string key);

    void Cancel(string key);
    void Failed(string key, string errorMessage);
    void Failed(string key, Exception exception);
    void Complete(string key, object? body);

    void LockTenant(string tenantKey, string nodeQueue, string tenantQueue);
    string? GetQueueForTenant(string tenantKey, string nodeQueue);
}

[Service(ServiceLifetime.Singleton)]
internal class InvokeStorage(IMemoryCache _cache) : IInvokeStorage
{
    private readonly string _prefix = "__LAPPY_CLUSTER_";

    public void Cancel(string key)
    {
        var tcs = _cache.Get<TaskCompletionSource<object?>>(TaskKey(key));
        var cts = _cache.Get<CancellationTokenSource>(CancelKey(key));

        tcs?.SetCanceled();
        cts?.Cancel();

        _cache.Remove(CancelKey(key));
        _cache.Remove(TaskKey(key));
    }

    public void Complete(string key, object? body)
    {
        var tcs = _cache.Get<TaskCompletionSource<object?>>(TaskKey(key));
        tcs?.SetResult(body);
        _cache.Remove(TaskKey(key));
    }

    public void Failed(string key, string errorMessage)
    {
        var tcs = _cache.Get<TaskCompletionSource<object?>>(TaskKey(key));
        tcs?.SetException(new ClusterException(errorMessage));
        _cache.Remove(TaskKey(key));
    }
    public void Failed(string key, Exception exception)
    {
        var tcs = _cache.Get<TaskCompletionSource<object?>>(TaskKey(key));
        tcs?.SetException(exception);
        _cache.Remove(TaskKey(key));
    }

    public CancellationToken CreateCancel(string key)
        => _cache.Set(
            CancelKey(key),
            new CancellationTokenSource(),
            TimeSpan.FromMinutes(5)
        ).Token;

    public Task<object?> StartTask(string key)
        => _cache.Set(
            TaskKey(key),
            new TaskCompletionSource<object?>(),
            TimeSpan.FromMinutes(5)
        ).Task;

    private string CancelKey(string key) => $"{_prefix}_CANCEL_{key}";
    private string TaskKey(string key) => $"{_prefix}_TASK_{key}";
    private string TenantKey(string key, string nodeQueue) => $"{_prefix}_TENANT_{key}_{nodeQueue}";

    public void LockTenant(string tenantKey, string nodeQueue, string tenantQueue)
        => _cache.Set(TenantKey(tenantKey, nodeQueue), tenantQueue, TimeSpan.FromMinutes(5));

    public string? GetQueueForTenant(string tenantKey, string nodeQueue) => _cache.Get<string>(TenantKey(tenantKey, nodeQueue));

}
