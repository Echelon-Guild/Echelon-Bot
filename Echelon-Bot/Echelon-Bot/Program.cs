using Azure.Data.Tables;
using Discord.Interactions;
using Discord.WebSocket;
using EchelonBot;
using EchelonBot.Services.WoW;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        // Always load environment variables (for Azure)
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<DiscordSocketClient>();
        services.AddSingleton(provider =>
        {
            var discord = provider.GetRequiredService<DiscordSocketClient>();
            return new InteractionService(discord);
        });
        services.AddHostedService<InteractionHandlingService>();
        services.AddHostedService<DiscordStartupService>();

        services.AddSingleton(provider =>
        {
            return new TableServiceClient(Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING"));
        });

        services.AddSingleton<BattleNetAuthService>();
        services.AddSingleton<WoWApiService>();

        services.AddHttpClient();

    })
    .Build();

await host.RunAsync();