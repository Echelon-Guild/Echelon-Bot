using Azure;
using Azure.Data.Tables;

namespace EchelonBot.Models.Entities
{
    public class EchelonEventEntity : ITableEntity
    {
        public string PartitionKey { get; set; }  // E.g., "Raid", "Mythic", "Meeting"
        public string RowKey { get; set; }  // Unique event ID
        public string EventName { get; set; }
        public DateTimeOffset EventDateTime { get; set; }
        public string Organizer { get; set; }
        public ulong MessageId { get; set; }

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }

}
