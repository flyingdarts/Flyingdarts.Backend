namespace Flyingdarts.Business.X01;
public record CreateX01GameDartCommand(string RoomId, long GameId) : IRequest<GameDart>;
public record CreateX01GameDartCommandHandler(X01Service X01Service) : IRequestHandler<CreateX01GameDartCommand, GameDart>
{
    public Task<GameDart> Handle(CreateX01GameDartCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}