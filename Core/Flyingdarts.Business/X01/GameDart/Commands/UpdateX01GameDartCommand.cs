namespace Flyingdarts.Business.X01;
public record UpdateX01GameDartCommand(string RoomId, long GameId) : IRequest<GameDart>;
public record UpdateX01GameDartCommandHandler(X01Service X01Service) : IRequestHandler<UpdateX01GameDartCommand, GameDart>
{
    public Task<GameDart> Handle(UpdateX01GameDartCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}