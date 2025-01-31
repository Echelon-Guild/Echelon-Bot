using Discord.Interactions;
using Discord.WebSocket;
using Echelon_Bot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("appsettings.json");
    })
    .ConfigureServices(services =>
    {
        // Add the discord client to services
        services.AddSingleton<DiscordSocketClient>();

        //services.AddSingleton<InteractionService>();        // Add the interaction service to services

        services.AddSingleton(provider =>
        {
            var discord = provider.GetRequiredService<DiscordSocketClient>();
            return new InteractionService(discord);
        });

        //services.AddHostedService<InteractionHandlingService>();    // Add the slash command handler
        services.AddSingleton(provider =>
        {
            var discord = provider.GetRequiredService<DiscordSocketClient>();
            var interaction = provider.GetRequiredService<InteractionService>();
            var config = provider.GetRequiredService<IConfiguration>();
            var serviceProvider = provider.GetRequiredService<IServiceProvider>();
            var logger = provider.GetRequiredService<ILogger<InteractionHandlingService>>();


            return new InteractionHandlingService(discord, interaction, serviceProvider, config, logger);
        });

        services.AddHostedService<DiscordStartupService>();         // Add the discord startup service
    })
    .Build();

await host.RunAsync();