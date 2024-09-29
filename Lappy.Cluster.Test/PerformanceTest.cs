using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lappy.Cluster.Test
{
    [TestFixture]
    internal class PerformanceTest
    {
        private readonly TestService service = new TestService();
        private readonly TestService serviceWithInterceptor = new TestService();

        [OneTimeSetUp]
        public void Setup()
        {
            var proxyGenerator = new ProxyGenerator();
            proxyGenerator.CreateClassProxyWithTarget<TestService>(service, new MyInterceptor());
        }

        [Test]
        public void TestOne()
        {
            service.Calculate(13);
            for (int i = 0; i < 10; i++)
            {
                var start = DateTime.UtcNow;

                service.Calculate(i);

                var end = DateTime.UtcNow;

                Console.WriteLine((end - start));
            }
        }


        [Test]
        public void TestTwo()
        {
            serviceWithInterceptor.Calculate(13);
            for (int i = 0; i < 10; i++)
            {
                var start = DateTime.UtcNow;

                serviceWithInterceptor.Calculate(i);

                var end = DateTime.UtcNow;

                Console.WriteLine((end - start));
            }
        }

    }

    public class TestService
    {
        public int Calculate(int input)
        {
            return input * 2;
        }
    }

    public class MyInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();
        }
    }
}
