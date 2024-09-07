namespace Lappy.Cluster.Handlers;

internal interface IHandler<TRequest>
{
    Task Handle(string correlationId, TRequest request);
}
