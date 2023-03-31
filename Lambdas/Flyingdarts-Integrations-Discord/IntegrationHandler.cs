using Amazon.Lambda.APIGatewayEvents;
using Flyingdarts.Shared;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Discord.Rest;

public class IntegrationHandler
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;
    private readonly InteractionService _interactionService;

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

        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Info,
            MessageCacheSize = 50
        });

        _interactionService = new InteractionService(_client.Rest);

        _commands = new CommandService(new CommandServiceConfig
        {
            // Again, log level:
            LogLevel = LogSeverity.Info,
            CaseSensitiveCommands = false,
        });


        var log = LoggingService.Instance(_client, _commands);
        _client.Log += log.LogAsync;
        _commands.Log += log.LogAsync;

        _services = ConfigureServices(_client, _commands);

        _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);

        _client.Ready += () => _interactionService.RegisterCommandsToGuildAsync(1013538880563183616);

    }
    public async Task<APIGatewayProxyResponse> Handle(byte[] discordBody)
    {
        try
        {
            if (!_client.Rest.IsValidHttpInteraction(publicKeyValue, signatureValue, timestampValue, discordBody))
            {
                return new APIGatewayProxyResponse { StatusCode = 400, Body = $"Invalid Interaction Signature! {discordBody}" };
            }

            await _client.LoginAsync(TokenType.Bot, tokenValue);
            await _client.StartAsync();

            

            RestInteraction interaction = await _client.Rest.ParseHttpInteractionAsync(publicKeyValue, signatureValue, timestampValue, discordBody);

            if (interaction is RestPingInteraction pingInteraction)
            {
                return new APIGatewayProxyResponse { StatusCode = 200, Body = pingInteraction.AcknowledgePing() };
            }

            return Responses.Created("Yhuy");
        }
        catch (Exception e)
        {
            return Responses.InternalServerError($"Failed to send message: {e.Message}");
        }
    }


    private static IServiceProvider ConfigureServices(DiscordSocketClient client, CommandService commandService)
    {
        var services = new ServiceCollection()
            .AddSingleton(new LoggingService(client, commandService));
        return services.BuildServiceProvider(true);
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