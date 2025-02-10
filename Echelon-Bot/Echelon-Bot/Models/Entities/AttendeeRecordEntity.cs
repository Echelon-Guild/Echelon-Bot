using Azure;
using Azure.Data.Tables;

namespace EchelonBot.Models.Entities
{
    public class AttendeeRecordEntity : ITableEntity
    {
        public string PartitionKey { get; set; }  // Event ID
        public string RowKey { get; set; }  // Unique id per attendee record
        public string DiscordName { get; set; }
        public string DiscordDisplayName { get; set; }
        public string Role { get; set; }
        public string? Class { get; set; }
        public string? Spec { get; set; }

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
