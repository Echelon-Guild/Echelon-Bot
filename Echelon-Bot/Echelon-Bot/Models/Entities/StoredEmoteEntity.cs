using Azure;
using Azure.Data.Tables;

namespace EchelonBot.Models.Entities
{
    public class StoredEmoteEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public string ClassName { get; set; }
        public string SpecName { get; set; }
        public string EmoteID { get; set; }
    }
}
