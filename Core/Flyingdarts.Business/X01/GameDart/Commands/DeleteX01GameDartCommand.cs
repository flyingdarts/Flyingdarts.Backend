namespace Flyingdarts.Business.X01;
public record DeleteX01GameDartCommand(long GameId) : IRequest;
public record DeleteX01GameDartCommandHandler(X01Service X01Service) : IRequestHandler<DeleteX01GameDartCommand>
{
    public Task Handle(DeleteX01GameDartCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}