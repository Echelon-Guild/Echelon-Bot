using Azure;
using Azure.Data.Tables;

namespace EchelonBot.Models.Entities
{
    public class EchelonUserEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "Users";
        public string RowKey { get; set; }
        public string DiscordName { get; set; }
        public string DiscordDisplayName { get; set; }
        public string TimeZone { get; set; }
        public string Class { get; set; }
        public string Spec { get; set; }
        

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
