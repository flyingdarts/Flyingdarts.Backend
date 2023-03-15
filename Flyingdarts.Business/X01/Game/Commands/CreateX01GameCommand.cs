namespace Flyingdarts.Business.X01;
public record CreateX01GameCommand(string RoomId, long GameId) : IRequest<Game>;
public record CreateX01GameCommandHandler(X01Service X01Service) : IRequestHandler<CreateX01GameCommand, Game>
{
    public Task<Game> Handle(CreateX01GameCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}