using Lappy.Cluster.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Lappy.Cluster.Handlers;

[Service(ServiceLifetime.Scoped)]
internal class EventHandler : IHandler<EventRequest>
{
    public Task Handle(string correlationId, EventRequest request)
    {
        throw new NotImplementedException();
    }
}
