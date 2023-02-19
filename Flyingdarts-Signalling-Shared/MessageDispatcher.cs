using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using System.Net;
using System.Text;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;

namespace Flyingdarts.Signalling.Shared;
public static class MessageDispatcher
{
    public static async Task DispatchMessage(ILambdaContext context, AmazonDynamoDBClient dynamoDbClient, AmazonApiGatewayManagementApiClient apiGatewayClient, string tableName, string data, string roomId)
    {
        // List all of the current connections. In a more advanced use case the table could be used to grab a group of connection ids for a chat group.
        var scanRequest = new ScanRequest
        {
            TableName = tableName,
            ProjectionExpression = $"ConnectionId,RoomId"
        };

        var scanResponse = await dynamoDbClient.ScanAsync(scanRequest);

        if (scanResponse.Items.Any())
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

            var connectedClientsInRoom = scanResponse.Items.Where(x => x[Fields.RoomId].S == roomId);

            // Loop through all of the connections and broadcast the message out to the connections.
            var count = 0;
            foreach (var item in connectedClientsInRoom)
            {
                var postConnectionRequest = new PostToConnectionRequest
                {
                    ConnectionId = item[Fields.ConnectionId].S,
                    Data = stream
                };

                try
                {
                    context.Logger.LogInformation($"Post to connection {count}: {postConnectionRequest.ConnectionId}");
                    stream.Position = 0;
                    await apiGatewayClient.PostToConnectionAsync(postConnectionRequest);
                    count++;
                }
                catch (AmazonServiceException e)
                {
                    // API Gateway returns a status of 410 GONE then the connection is no
                    // longer available. If this happens, delete the identifier
                    // from our DynamoDB table.
                    if (e.StatusCode == HttpStatusCode.Gone)
                    {
                        var ddbDeleteRequest = new DeleteItemRequest
                        {
                            TableName = tableName,
                            Key = new Dictionary<string, AttributeValue>
                        {
                            { Fields.ConnectionId, new AttributeValue {S = postConnectionRequest.ConnectionId }}
                        }
                        };

                        context.Logger.LogInformation($"Deleting gone connection: {postConnectionRequest.ConnectionId}");
                        await dynamoDbClient.DeleteItemAsync(ddbDeleteRequest);
                    }
                    else
                    {
                        context.Logger.LogInformation($"Error posting message to {postConnectionRequest.ConnectionId}: {e.Message}");
                        context.Logger.LogInformation(e.StackTrace);
                    }
                }
            }
        }
    }
}

