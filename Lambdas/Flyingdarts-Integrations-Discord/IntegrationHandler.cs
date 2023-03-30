using Amazon.Lambda.APIGatewayEvents;
using Flyingdarts.Requests;
using Flyingdarts.Shared;
using System;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

public class IntegrationHandler
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;

    public IntegrationHandler() { }

    public IntegrationHandler(APIGatewayProxyRequest request)
    {
        string signatureValue, timestampValue, publicKeyValue, tokenValue;
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

        if (!_client.Rest.IsValidHttpInteraction(publicKeyValue, signatureValue, timestampValue, Encoding.UTF8.GetBytes(request.Body)))
            throw new InvalidHttpInteractionException("Invalid http interaction!");

        _commands = new CommandService(new CommandServiceConfig
        {
            // Again, log level:
            LogLevel = LogSeverity.Info,

            // There's a few more properties you can set,
            // for example, case-insensitive commands.
            CaseSensitiveCommands = false,
        });

        // Subscribe the logging handler to both the client and the CommandService.
        var log = LoggingService.Instance(_client, _commands);
        _client.Log += log.LogAsync;
        _commands.Log += log.LogAsync;

        // Setup your DI container.
        _services = ConfigureServices(_client, _commands);

        Task.Run(() => MainAsync(tokenValue).GetAwaiter().GetResult());
    }

    public async Task<APIGatewayProxyResponse> Handle(IAmAMessage<DiscordIntegrationRequest> request)
    {
        try
        {
            // Check if the request is a ping event
            if (request.Message is not null && request.Message.Type != 1)
                return Responses.InternalServerError("Ping pong failure");

            // Return a response with a serialized ping-pong object
            return Responses.Created(JsonSerializer.Serialize(DiscordIntegrationRequest.PingPong));
        }
        catch (Exception e)
        {
            // If an exception is caught, return an error response with the error message
            return Responses.InternalServerError($"Failed to send message: {e.Message}");
        }
    }

    // If any services require the client, or the CommandService, or something else you keep on hand,
    // pass them as parameters into this method as needed.
    // If this method is getting pretty long, you can seperate it out into another file using partials.
    private static IServiceProvider ConfigureServices(DiscordSocketClient client, CommandService commandService)
    {
        var services = new ServiceCollection()
            // Repeat this for all the service classes
            // and other dependencies that your commands might need.
            .AddSingleton(new LoggingService(client, commandService));

        // When all your required services are in the collection, build the container.
        // Tip: There's an overload taking in a 'validateScopes' bool to make sure
        // you haven't made any mistakes in your dependency graph.
        return services.BuildServiceProvider(true);
    }

    // Example of a logging handler. This can be re-used by addons
    // that ask for a Func<LogMessage, Task>.
    private static Task Log(LogMessage message)
    {
        Console.ResetColor();
        // If you get an error saying 'CompletedTask' doesn't exist,
        // your project is targeting .NET 4.5.2 or lower. You'll need
        // to adjust your project's target framework to 4.6 or higher
        // (instructions for this are easily Googled).
        // If you *need* to run on .NET 4.5 for compat/other reasons,
        // the alternative is to 'return Task.Delay(0);' instead.
        return Task.CompletedTask;
    }
    private async Task MainAsync(string token)
    {
        // Centralize the logic for commands into a separate method.
        await InitCommands();

        // Login and connect.
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        // Wait infinitely so your bot actually stays connected.
        await Task.Delay(Timeout.Infinite);
    }

    private async Task InitCommands()
    {
        // Either search the program and add all Module classes that can be found.
        // Module classes MUST be marked 'public' or they will be ignored.
        // You also need to pass your 'IServiceProvider' instance now,
        // so make sure that's done before you get here.
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        // Or add Modules manually if you prefer to be a little more explicit:
        //await _commands.AddModuleAsync<SomeModule>(_services);
        // Note that the first one is 'Modules' (plural) and the second is 'Module' (singular).

        // Subscribe a handler to see if a message invokes a command.
        _client.MessageReceived += HandleCommandAsync;
    }

    private async Task HandleCommandAsync(SocketMessage arg)
    {
        // Bail out if it's a System Message.
        var msg = arg as SocketUserMessage;
        if (msg == null) return;

        // We don't want the bot to respond to itself or other bots.
        if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot) return;

        // Create a number to track where the prefix ends and the command begins
        int pos = 0;
        // Replace the '!' with whatever character
        // you want to prefix your commands with.
        // Uncomment the second half if you also want
        // commands to be invoked by mentioning the bot instead.
        if (msg.HasCharPrefix('!', ref pos) /* || msg.HasMentionPrefix(_client.CurrentUser, ref pos) */)
        {
            // Create a Command Context.
            var context = new SocketCommandContext(_client, msg);

            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully).
            var result = await _commands.ExecuteAsync(context, pos, _services);

            // Uncomment the following lines if you want the bot
            // to send a message if it failed.
            // This does not catch errors from commands with 'RunMode.Async',
            // subscribe a handler for '_commands.CommandExecuted' to see those.
            //if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
            //    await msg.Channel.SendMessageAsync(result.ErrorReason);
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