using Lappy.Cluster.Helpers;
using Lappy.Core.Pool;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RabbitMQ.Client;
using System.Reflection;

namespace Lappy.Cluster.Test.Helpers;

internal static class TestHelper
{
    public static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddTransient<ITestingService, TestingService>();
        services.AddScoped<TestRequestContext>();
        services.AddCluster(c => c.WithNodeName("TEST").WithContext<TestRequestContext>());
        services.AddPooled(provider =>
        {
            var model = Substitute.For<IModel>();
            model.CreateBasicProperties().Returns(Substitute.For<IBasicProperties>());
            model
                .WhenForAnyArgs(d => d.BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<IBasicProperties>(), Arg.Any<ReadOnlyMemory<byte>>()))
                .Do(call =>
                {
                    var args = call.Args();
                    var consumer = provider.GetRequiredService<ClusterConsumer>();
                    _ = consumer.HandleBasicDeliver(
                        string.Empty,
                        0,
                        (bool)args[2],
                        (string)args[0],
                        (string)args[1],
                        (IBasicProperties)args[3],
                        (ReadOnlyMemory<byte>)args[4]
                    );
                });
            return model;
        });
        return services.BuildServiceProvider();
    }
}
