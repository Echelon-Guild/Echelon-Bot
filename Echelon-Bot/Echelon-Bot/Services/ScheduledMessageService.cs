using Azure.Data.Tables;
using Discord;
using Discord.WebSocket;
using EchelonBot.Models;
using EchelonBot.Models.Entities;
using Microsoft.Extensions.Hosting;

namespace EchelonBot.Services
{
    public class ScheduledMessageService : BackgroundService
    {
        private readonly DiscordSocketClient _client;
        private readonly TableClient _scheduledMessageTable;

        private readonly EventStorageService _eventStorageService;

        private readonly EmbedFactory _embedFactory;

        public ScheduledMessageService(DiscordSocketClient client, TableServiceClient tableServiceClient, EmbedFactory embedFactory, EventStorageService eventStorageService)
        {
            _client = client;
            _scheduledMessageTable = tableServiceClient.GetTableClient("ScheduledMessages");
            _scheduledMessageTable.CreateIfNotExists();

            _eventStorageService = eventStorageService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTimeOffset.UtcNow;

                var entities = _scheduledMessageTable.Query<ScheduledMessageEntity>(m => m.SendTime <= now).ToList();

                foreach (var msg in entities)
                {
                    var user = _client.GetUser(msg.UserId);
                    if (user != null)
                    {
                        var dmChannel = await user.CreateDMChannelAsync();
                        // await dmChannel.SendMessageAsync(msg.Message);

                        Guid eventId = Guid.Parse(msg.EventId);

                        EchelonEvent? ecEvent = _eventStorageService.GetEvent(eventId);

                        // If we can't find the event we can't really do anything else.
                        if (ecEvent is null)
                        {
                            await _scheduledMessageTable.DeleteEntityAsync(msg.PartitionKey, msg.RowKey);
                            continue;
                        }

                        Embed embed = _embedFactory.CreateEventEmbed(ecEvent);

                        await dmChannel.SendMessageAsync(msg.Message, embed: embed);

                        await _scheduledMessageTable.DeleteEntityAsync(msg.PartitionKey, msg.RowKey);
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }


    }
}

