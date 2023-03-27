using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Flyingdarts.Persistance;
using Flyingdarts.Services.X01;
using Flyingdarts.Shared;
using Microsoft.Extensions.Options;
using Moq;

namespace Flyingdarts.Games.X01.OnCreate.Tests
{
    public class Tests
    {
        private X01Service _service;
        private Mock<IAmazonDynamoDB> _mockClient;
        private DynamoDBContextConfig _dynamoDbContextConfig;
        private DynamoDBContext _dynamoDbContext;

        private Guid UnitTestPlayerId => Guid.Parse("000024ff-0000-0000-0000-e8b954400001");

        [SetUp]
        public void Setup()
        {
            _mockClient = new Mock<IAmazonDynamoDB>();

            // 3. Create a DynamoDB context using the mock client
            _dynamoDbContextConfig = new DynamoDBContextConfig { ConsistentRead = true };
            _dynamoDbContext = new DynamoDBContext(_mockClient.Object, _dynamoDbContextConfig);

            _service = new X01Service(
                Options.Create(new ApplicationOptions { DynamoDbTable = "ApplicationTable" }),
                new AmazonDynamoDBClient());
        }
        
        [Test]
        public async Task EnsureNewGameCanBeCreated()
        {
            var game = Game.Create(1, X01GameSettings.Create(1, 5), "UnitTestRoomId");
            async Task PutNewGame() => await _service.PutGame(game);
            Assert.DoesNotThrowAsync(PutNewGame);
        }

        [Test]
        public async Task EnsurePlayerCanOnlyHaveOneOnGoingGame()
        {
            var player = GamePlayer.Create(DateTime.UtcNow.Ticks, UnitTestPlayerId);
            async Task PutDuplicatePlayer() => await _service.PutGamePlayer(player);
            Assert.ThrowsAsync<Exception>(PutDuplicatePlayer);
        }
    }
}

public class DuplicateEntityException : Exception { }
public class InvalidEntryException : Exception { }