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
        private readonly TableClient _eventTable;

        private readonly EmbedFactory _embedFactory;

        public ScheduledMessageService(DiscordSocketClient client, TableServiceClient tableServiceClient, EmbedFactory embedFactory)
        {
            _client = client;
            _scheduledMessageTable = tableServiceClient.GetTableClient("ScheduledMessages");
            _scheduledMessageTable.CreateIfNotExists();

            _eventTable = tableServiceClient.GetTableClient("EchelonEvents");
            _embedFactory = embedFactory;
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

                        string rowKey = msg.EventId;

                        EchelonEventEntity event_ = _eventTable.Query<EchelonEventEntity>(e => e.RowKey == rowKey).First();

                        EventType eventType = Enum.Parse<EventType>(event_.PartitionKey);

                        EchelonEvent ecEvent = new(event_.EventName, event_.EventDescription, event_.Organizer, event_.ImageUrl, event_.Footer, event_.EventDateTime, eventType);

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

