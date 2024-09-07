using Lappy.Cluster.Handlers;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Lappy.Cluster.Helpers;

internal class ReflectionHelper
{
    private static readonly ReflectionHelper _instance = new();

    private ReflectionHelper() { }

    public static Task Handle(IServiceProvider serviceProvider, string correlationId, object? request)
    {
        if (request == null) return Task.CompletedTask;

        var method = typeof(ReflectionHelper)
            .GetMethod(nameof(HandlePrivate), BindingFlags.NonPublic | BindingFlags.Instance)
            ?.MakeGenericMethod(request.GetType());

        return (Task?)method?.Invoke(_instance, [serviceProvider, correlationId, request]) ?? Task.CompletedTask;
    }

    private Task HandlePrivate<T>(IServiceProvider serviceProvider, string correlationId, T request)
        => serviceProvider.GetRequiredService<IHandler<T>>().Handle(correlationId, request);

    public static Task CastTask(Task<object?> task, Type returnType)
    {
        var inner = returnType.GenericTypeArguments.FirstOrDefault();
        if (inner == null)
        {
            return task;
        }

        var method = typeof(ReflectionHelper)
            .GetMethod(nameof(CastTaskPrivate), BindingFlags.Instance | BindingFlags.NonPublic)
            ?.MakeGenericMethod(inner);

        return (Task?)method?.Invoke(_instance, [task]) ?? Task.CompletedTask;
    }

    private async Task<T?> CastTaskPrivate<T>(Task<object?> t) => (T?)await t;
}
