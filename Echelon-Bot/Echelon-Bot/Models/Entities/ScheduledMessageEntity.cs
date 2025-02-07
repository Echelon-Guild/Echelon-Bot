using Azure;
using Azure.Data.Tables;

namespace EchelonBot.Models.Entities
{
    public class ScheduledMessageEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "ScheduledMessages";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public ulong UserId { get; set; }
        public string EventId { get; set; }
        public string Message { get; set; }
        public DateTimeOffset SendTime { get; set; }
    }

}
