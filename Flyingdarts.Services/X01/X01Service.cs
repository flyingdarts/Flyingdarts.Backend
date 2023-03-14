using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Flyingdarts.Persistance;
using Flyingdarts.Shared;
using Flyingdarts.Signalling.Shared;
using Microsoft.Extensions.Options;

namespace Flyingdarts.Services.X01;

public class X01Service
    {
        public IDynamoDBContext DbContext { get; }
        public IOptions<ApplicationOptions> ApplicationOptions { get; }
        public CancellationToken CancellationToken { get; }
        public X01Service(IDynamoDBContext dbContext, IOptions<ApplicationOptions> applicationOptions, CancellationToken cancellationToken = default)
        {
            DbContext = dbContext;
            ApplicationOptions = applicationOptions;
            CancellationToken = cancellationToken;
        }
        public Game Game { get; set; }
        public List<GamePlayer> Players { get; set; }
        public List<GameDart> Darts { get; set; }

        public async Task Initialize(long gameId)
        {
            Game = DbContext.FromQueryAsync<Game>(GetGameQueryConfig(gameId), ApplicationOptions.Value.ToOperationConfig()).GetRemainingAsync(CancellationToken).Result.Single();
            Players = await DbContext.FromQueryAsync<GamePlayer>(GetPlayersInGameQueryConfig(), ApplicationOptions.Value.ToOperationConfig()).GetRemainingAsync(CancellationToken);
            Darts = await DbContext.FromQueryAsync<GameDart>(GetDartsThrownInGameQueryConfig(), ApplicationOptions.Value.ToOperationConfig()).GetRemainingAsync(CancellationToken);
        }

        public List<Guid> GetPlayersThatShouldThrowAgain()
        {
            var returnValue = new List<Guid>();

            IEnumerable<PlayerDartScores> PlayerDartScores = Darts.OrderBy(x => x.CreatedAt)
                                                                  .GroupBy(x => x.PlayerId)
                                                                  .Select(x =>
                                                                      new PlayerDartScores
                                                                      {
                                                                          PlayerId = x.Key,
                                                                          Scores = x.ToArray()
                                                                                    .Split(3)
                                                                                    .Select(x =>
                                                                                        new DartScore
                                                                                        {
                                                                                            Values = x.Select(x => x.Score)
                                                                                                      .ToArray()
                                                                                        })
                                                                      });

            // All players must have a darts count of MOD 3 == 0 AND an equal count of grouped split arrays with length 3
            var playersHaveSameArrayLength = PlayerDartScores.All(x => x.Scores.Count() == PlayerDartScores.First().Scores.Count());
            var playersLastSetContainThreeDarts = PlayerDartScores.All(x => x.Scores.Last().Values.Count() % 3 == 0);

            if (playersHaveSameArrayLength && playersLastSetContainThreeDarts)
            {
                returnValue = PlayerDartScores.Select(x =>
                                                    new
                                                    {
                                                        Id = x.PlayerId,
                                                        LastScore = x.Scores.Last()
                                                    })
                                                .GroupBy(x => x.LastScore.Score)
                                                .Where(x => x.Count() > 1)
                                                .SelectMany(x => x.Select(x => x.Id))
                                                .ToList();
            }
            return returnValue;
        }
        private QueryOperationConfig GetGameQueryConfig(long gameId)
        {
            var queryFilter = new QueryFilter("PK", QueryOperator.Equal, Constants.Game);
            queryFilter.AddCondition("SK", QueryOperator.BeginsWith, $"{gameId}#");
            return new QueryOperationConfig { Filter = queryFilter };
        }
        private QueryOperationConfig GetDartsThrownInGameQueryConfig()
        {
            var queryFilter = new QueryFilter("PK", QueryOperator.Equal, Constants.GameDart);
            queryFilter.AddCondition("SK", QueryOperator.BeginsWith, $"{Game.GameId}#");
            return new QueryOperationConfig { Filter = queryFilter };
        }
        private QueryOperationConfig GetPlayersInGameQueryConfig()
        {
            var queryFilter = new QueryFilter("PK", QueryOperator.Equal, Constants.GamePlayer);
            queryFilter.AddCondition("SK", QueryOperator.BeginsWith, $"{Game.GameId}#");
            return new QueryOperationConfig { Filter = queryFilter };
        }
    }