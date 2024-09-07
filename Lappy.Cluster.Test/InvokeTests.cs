using Lappy.Cluster.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Lappy.Cluster.Test
{
    [TestFixture]
    public class InvokeTests
    {
        private ServiceProvider _serviceProvider;
        private ITestingService _testingService;
        private ITestingService _original = new TestingService(new TestRequestContext());

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

        [Test]
        public void PerformanceTest()
        {
            RunMultipleTimes(() => _testingService.DoSomething());
        }

        [Test]
        public void PerformanceTestOriginal()
        {
            RunMultipleTimes(() => _original.DoSomething());
        }

        [Test]
        public async Task PerformanceTestAsync()
        {
            await RunMultipleTimesAsync(() => _testingService.DoSomethingAsync(), 25);
        }


        [Test]
        public async Task PerformanceTestOriginalAsync()
        {
            await RunMultipleTimesAsync(() => _original.DoSomethingAsync(), 25);
        }

        [Test]
        public void Sum()
        {
            RunMultipleTimes(() =>
            {
                var result = _testingService.GetSum(4, 1);
                Assert.That(result, Is.EqualTo(5));
            });
        }

        [Test]
        public async Task SumAsync()
        {
            await RunMultipleTimesAsync(async () =>
            {
                var result = await _testingService.GetSumAsync(4, 1, default);
                Assert.That(result, Is.EqualTo(5));
            });
        }

        private static void RunMultipleTimes(Action action, int totalCount = 25)
        {
            var total = 0D;
            var count = 0;
            for (var i = 0; i < totalCount; i++)
            {
                var start = DateTime.UtcNow;
                action();
                var end = DateTime.UtcNow;
                if (i > 2) // Skip first load
                {
                    total += (end - start).TotalMilliseconds;
                    count++;
                }
            }
            Console.WriteLine("Average " + Math.Round(total / count, 4) + " ms");
        }

        private static async Task RunMultipleTimesAsync(Func<Task> action, int totalCount = 25)
        {
            var total = 0D;
            var count = 0;
            for (var i = 0; i < totalCount; i++)
            {
                var start = DateTime.UtcNow;
                await action();
                var end = DateTime.UtcNow;
                if (i > 2) // Skip first load
                {
                    total += (end - start).TotalMilliseconds;
                    count++;
                }
            }
            Console.WriteLine("Average " + Math.Round(total / count, 4) + " ms");
        }
    }
}