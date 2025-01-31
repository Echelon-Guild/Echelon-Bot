using Discord.Interactions;
using Discord.WebSocket;
using EchelonBot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("appsettings.json");       // Add the config file to IConfiguration variables
    })
    .ConfigureServices(services =>
    {
        services.AddSingleton<DiscordSocketClient>();       // Add the discord client to services
        services.AddSingleton(provider =>
        {
            var discord = provider.GetRequiredService<DiscordSocketClient>();
            return new InteractionService(discord);
        });
        services.AddHostedService<InteractionHandlingService>();    // Add the slash command handler
        services.AddHostedService<DiscordStartupService>();         // Add the discord startup service
    })
    .Build();

await host.RunAsync();