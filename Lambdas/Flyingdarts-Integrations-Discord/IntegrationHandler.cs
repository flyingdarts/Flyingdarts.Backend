using Amazon.Lambda.APIGatewayEvents;
using Flyingdarts.Shared;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Auth.AccessControlPolicy.ActionIdentifiers;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Discord.Rest;
using Flyingdarts.Requests;
using Newtonsoft.Json;

public class IntegrationHandler
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;
    public string signatureValue, timestampValue;
    public string publicKeyValue, tokenValue;

    public IntegrationHandler() { }

    public IntegrationHandler(APIGatewayProxyRequest request)
    {
        // Ensure header contains the signature value
        if (!request.Headers.TryGetValue("x-signature-ed25519", out signatureValue))
            throw new InvalidHttpInteractionException("Signature header not present!");
        // Ensure custom header with signature timestamp 
        if (!request.Headers.TryGetValue("x-signature-timestamp", out timestampValue))
            throw new InvalidHttpInteractionException("Signature header not present!");
        if (PublicKeyIsNotPresent(out publicKeyValue) || TokenIsNotPresent(out tokenValue))
            throw new InvalidHttpInteractionException("PublicKey or Token not present!");

        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            // How much logging do you want to see?
            LogLevel = LogSeverity.Info,

            // If you or another service needs to do anything with messages
            // (eg. checking Reactions, checking the content of edited/deleted messages),
            // you must set the MessageCacheSize. You may adjust the number as needed.
            MessageCacheSize = 50,

            // If your platform doesn't have native WebSockets,
            // add Discord.Net.Providers.WS4Net from NuGet,
            // add the `using` at the top, and uncomment this line:
            // WebSocketProvider = WS4NetProvider.Instance
        });

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
    }
    public async Task<APIGatewayProxyResponse> Handle(byte[] discordBody)
    {
        try
        {
            if (!_client.Rest.IsValidHttpInteraction(publicKeyValue, signatureValue, timestampValue, discordBody))
            {
                return new APIGatewayProxyResponse { StatusCode = 400, Body = $"Invalid Interaction Signature! {discordBody}" };
            }

            RestInteraction interaction = await _client.Rest.ParseHttpInteractionAsync(publicKeyValue, signatureValue, timestampValue, discordBody);

            if (interaction is RestPingInteraction pingInteraction)
            {
                return new APIGatewayProxyResponse { StatusCode = 200, Body = pingInteraction.AcknowledgePing() };
            }
            // handle command 
            await MainAsync(tokenValue);

            // Return a response with a serialized ping-pong object
            return Responses.Created("Yhuy");
        }
        catch (Exception e)
        {
            // If an exception is caught, return an error response with the error message
            return Responses.InternalServerError($"Failed to send message: {e.Message}");
        }
    }
    private static IServiceProvider ConfigureServices(DiscordSocketClient client, CommandService commandService)
    {
        var services = new ServiceCollection()
            .AddSingleton(new LoggingService(client, commandService));
        return services.BuildServiceProvider(true);
    }

    private async Task MainAsync(string token)
    {
        await InitCommands();

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(Timeout.Infinite);
    }

    public async Task SlashCommandHandler(SocketSlashCommand command)
    {
        await command.RespondAsync($"You executed {command.Data.Name}");
    }
    public async Task Client_Ready()
    {
        var guild = _client.GetGuild(1013538880563183616);
        var guildCommand = new SlashCommandBuilder();
        guildCommand.WithName("huur");
        guildCommand.WithDescription("duur");

        var globalCommand = new SlashCommandBuilder();
        globalCommand.WithName("peb");
        globalCommand.WithName("cac");

        try
        {
            await guild.CreateApplicationCommandAsync(guildCommand.Build());

            await _client.CreateGlobalApplicationCommandAsync(globalCommand.Build());
        }
        catch (HttpException e)
        {
            await guild.DefaultChannel.SendMessageAsync(JsonConvert.SerializeObject(e.Errors, Formatting.Indented));
            throw;
        }
    }
    private async Task InitCommands()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        _client.Ready += Client_Ready;
        _client.MessageReceived += HandleCommandAsync;
        _client.SlashCommandExecuted += SlashCommandHandler;
    }

    private async Task HandleCommandAsync(SocketMessage arg)
    {
        var msg = arg as SocketUserMessage;
        if (msg == null) return;

        if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot) return;

        int pos = 0;
     
        if (msg.HasCharPrefix('!', ref pos))
        {
            var context = new SocketCommandContext(_client, msg);
            await context.Channel.SendMessageAsync("Ah yeet!");
        }
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