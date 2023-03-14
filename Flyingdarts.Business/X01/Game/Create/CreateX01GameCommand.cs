// ReSharper disable once CheckNamespace

using MediatR;

namespace Flyingdarts.Business.X01;

public record CreateX01GameCommand(
    int PlayerCount, int Sets, int Leg,
    bool DoubleIn, bool DoubleOut,
    int StartingScore, List<Guid> PlayerIds,
    string roomId
) : IRequest<CreateX01GameResult>;