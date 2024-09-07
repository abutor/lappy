using Lappy.Cluster.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Lappy.Cluster.Test;

internal class CancellationTests
{
    private ServiceProvider _serviceProvider;
    private ITestingService _testingService;

    [OneTimeSetUp]
    public void Setup()
    {
        _serviceProvider = TestHelper.CreateServiceProvider();
        _testingService = _serviceProvider.GetRequiredService<ITestingService>();
    }

    [OneTimeTearDown]
    public void TeadRown()
    {
        _serviceProvider.Dispose();
    }

    [Test, MaxTime(500)]
    public void Cancel()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(30);
        Assert.ThrowsAsync<TaskCanceledException>(() => _testingService.Cancelable(TimeSpan.FromSeconds(5), cts.Token));
    }
}
