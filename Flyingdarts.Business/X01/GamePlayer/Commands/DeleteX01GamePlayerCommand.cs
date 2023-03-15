namespace Flyingdarts.Business.X01;
public record DeleteX01GamePlayerCommand(long GameId) : IRequest;
public record DeleteX01GamePlayerCommandHandler(X01Service X01Service) : IRequestHandler<DeleteX01GameCommand>
{
    public Task Handle(DeleteX01GameCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}