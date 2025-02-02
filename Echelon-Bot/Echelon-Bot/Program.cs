using Azure.Data.Tables;
using Discord.Interactions;
using Discord.WebSocket;
using EchelonBot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        if (File.Exists("appsettings.json"))
        {
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        }

        // Always load environment variables (for Azure)
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<DiscordSocketClient>();       // Add the discord client to services
        services.AddSingleton(provider =>
        {
            var discord = provider.GetRequiredService<DiscordSocketClient>();
            return new InteractionService(discord);
        });
        services.AddHostedService<InteractionHandlingService>();    // Add the slash command handler
        services.AddHostedService<DiscordStartupService>();  // Add the discord startup service

        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            string storageConnectionString = config["AzureTableStorage:ConnectionString"] ?? Environment.GetEnvironmentVariable("ACR_TableStor");
            return new TableServiceClient(storageConnectionString);
        });

    })
    .Build();

await host.RunAsync();