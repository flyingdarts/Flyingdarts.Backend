namespace Flyingdarts.Business.X01;
public record UpdateX01GameCommand(string RoomId, long GameId) : IRequest<Game>;
public record UpdateX01GameCommandHandler(X01Service X01Service) : IRequestHandler<UpdateX01GameCommand, Game>
{
    public Task<Game> Handle(UpdateX01GameCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}