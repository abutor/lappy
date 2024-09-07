using Lappy.Cluster.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Lappy.Cluster.Test
{
    [TestFixture]
    internal class ContextTests
    {
        ServiceProvider _provider;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _provider = TestHelper.CreateServiceProvider();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }

        [Test]
        public void TestContextSet([Random(5)] int range)
        {
            using var scope = _provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TestRequestContext>();
            var service = scope.ServiceProvider.GetRequiredService<ITestingService>();
            context.RequestId = range;
            var result = service.GetFromContext();
            Assert.That(result, Is.EqualTo(range));
        }

        [Test]
        public async Task TestContextSetAsync([Random(5)] int range)
        {
            using var scope = _provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TestRequestContext>();
            var service = scope.ServiceProvider.GetRequiredService<ITestingService>();
            context.RequestId = range;
            var result = await service.GetFromContextAsync();
            Assert.That(result, Is.EqualTo(range));
        }
    }
}
