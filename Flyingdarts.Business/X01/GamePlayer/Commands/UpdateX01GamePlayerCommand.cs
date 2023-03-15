namespace Flyingdarts.Business.X01;
public record UpdateX01GamePlayerCommand(string RoomId, long GameId) : IRequest<Game>;
public record UpdateX01GamePlayerCommandHandler(X01Service X01Service) : IRequestHandler<UpdateX01GamePlayerCommand, Game>
{
    public Task<Game> Handle(UpdateX01GamePlayerCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}