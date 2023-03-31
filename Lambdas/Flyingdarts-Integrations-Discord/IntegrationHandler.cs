using Amazon.Lambda.APIGatewayEvents;
using Flyingdarts.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Discord.Rest;

public class IntegrationHandler
{
    private readonly IServiceProvider _services;

    public string signatureValue, timestampValue;
    public string publicKeyValue, tokenValue;

    public IntegrationHandler() { }

    public IntegrationHandler(APIGatewayProxyRequest request)
    {
        if (!request.Headers.TryGetValue("x-signature-ed25519", out signatureValue))
            throw new InvalidHttpInteractionException("Signature header not present!");
        if (!request.Headers.TryGetValue("x-signature-timestamp", out timestampValue))
            throw new InvalidHttpInteractionException("Signature header not present!");
        if (PublicKeyIsNotPresent(out publicKeyValue) || TokenIsNotPresent(out tokenValue))
            throw new InvalidHttpInteractionException("PublicKey or Token not present!");

        _services = ConfigureServices();
    }
    private async Task LogAsync(LogMessage message) => Console.WriteLine(message.ToString());
    public async Task<APIGatewayProxyResponse> Handle(byte[] discordBody)
    {
        try
        {
            var logClient = new AmazonCloudWatchLogsClient();
            var client = _services.GetRequiredService<DiscordSocketClient>();
            var logGroupName = "/aws/Kakakakakakakakakakaka";
            var logStreamName = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            var existing = await logClient
                .DescribeLogGroupsAsync(new DescribeLogGroupsRequest()
                    { LogGroupNamePrefix = logGroupName });
            var logGroupExists = existing.LogGroups.Any(l => l.LogGroupName == logGroupName);
            if (!logGroupExists)
                await logClient.CreateLogGroupAsync(new CreateLogGroupRequest(logGroupName));
            await logClient.CreateLogStreamAsync(new CreateLogStreamRequest(logGroupName, logStreamName));
            await logClient.PutLogEventsAsync(new PutLogEventsRequest()
            {
                LogGroupName = logGroupName,
                LogStreamName = logStreamName,
                LogEvents = new List<InputLogEvent>()
                {
                    new()
                    {
                        Message = $"Kakakakakakakakakakaka",
                        Timestamp = DateTime.UtcNow
                    }
                }
            });

            client.Log += LogAsync;

            await _services.GetRequiredService<InteractionHandler>().InitializeAsync();

            await client.LoginAsync(TokenType.Bot, tokenValue);

            await client.StartAsync();

            if (!client.Rest.IsValidHttpInteraction(publicKeyValue, signatureValue, timestampValue, discordBody))
            {
                return new APIGatewayProxyResponse { StatusCode = 400, Body = $"Invalid Interaction Signature! {discordBody}" };
            }

            RestInteraction interaction = await client.Rest.ParseHttpInteractionAsync(publicKeyValue, signatureValue, timestampValue, discordBody);

            if (interaction is RestPingInteraction pingInteraction)
            {
                return new APIGatewayProxyResponse { StatusCode = 200, Body = pingInteraction.AcknowledgePing() };
            }

            return Responses.Created("Yhuy");
        }
        catch (Exception e)
        {
            return Responses.InternalServerError(e.ToString());
        }
    }


    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection()
            .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
                { LogLevel = LogSeverity.Info, MessageCacheSize = 50 }))
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<InteractionHandler>();
        return services.BuildServiceProvider();
    }
    
    private bool PublicKeyIsNotPresent(out string publicKeyValue)
    {
        publicKeyValue = Environment.GetEnvironmentVariable("DISCORD_BOT_PUBLIC_KEY");
        return publicKeyValue is null;
    }
    private bool TokenIsNotPresent(out string tokenValue)
    {
        tokenValue = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        return tokenValue is null;
    }
    
}