using Lappy.Cluster.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Lappy.Cluster.Test
{
    [TestFixture]
    internal class ExceptionTests
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

        [Test]
        public void Exception()
        {
            Assert.Throws<ApplicationException>(() => _testingService.WithException());
        }

        [Test]
        public void ExceptionAsync()
        {
            Assert.ThrowsAsync<ApplicationException>(async () => await _testingService.WithException(default));
        }
    }
}
