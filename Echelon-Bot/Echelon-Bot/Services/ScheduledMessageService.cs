using Azure.Data.Tables;
using Discord.WebSocket;
using EchelonBot.Models.Entities;
using Microsoft.Extensions.Hosting;

namespace EchelonBot
{
    public class ScheduledMessageService : BackgroundService
    {
        private readonly DiscordSocketClient _client;
        private readonly TableClient _tableClient;

        public ScheduledMessageService(DiscordSocketClient client, TableServiceClient tableServiceClient)
        {
            _client = client;
            _tableClient = tableServiceClient.GetTableClient("ScheduledMessages");
            _tableClient.CreateIfNotExists();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTimeOffset.UtcNow;

                var entities = _tableClient.Query<ScheduledMessageEntity>(m => m.SendTime <= now).ToList();

                foreach (var msg in entities)
                {
                    var user = _client.GetUser(msg.UserId);
                    if (user != null)
                    {
                        var dmChannel = await user.CreateDMChannelAsync();
                        await dmChannel.SendMessageAsync(msg.Message);
                        await _tableClient.DeleteEntityAsync(msg.PartitionKey, msg.RowKey);
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}

