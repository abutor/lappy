using Lappy.Core.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Lappy.Cluster.Test.Helpers;

[RemoteService]
public interface ITestingService
{
    void DoSomething();
    Task DoSomethingAsync();
    int GetSum(int number1, int number2);
    Task<int> GetSumAsync(int number1, int number2, CancellationToken token);

    void WithException();
    Task WithException(CancellationToken token);

    Task Cancelable(TimeSpan delay, CancellationToken token);

    int GetFromContext();
    Task<int> GetFromContextAsync();
}

[Service(ServiceLifetime.Singleton)]
public class TestingService(TestRequestContext context) : ITestingService
{
    public Task DoSomethingAsync()
    {
        return Task.CompletedTask;
    }

    public void DoSomething()
    {
    }

    public int GetSum(int number1, int number2)
    {
        return number1 + number2;
    }

    public Task<int> GetSumAsync(int number1, int number2, CancellationToken token)
    {
        return Task.FromResult(number1 + number2);
    }

    public void WithException()
    {
        throw new ApplicationException();
    }

    public Task WithException(CancellationToken token)
    {
        throw new ApplicationException();
    }

    public Task Cancelable(TimeSpan delay, CancellationToken token)
    {
        return Task.Delay(delay, token);
    }

    public int GetFromContext()
    {
        return context.RequestId;
    }

    public Task<int> GetFromContextAsync()
    {
        return Task.FromResult(context.RequestId);
    }
}
