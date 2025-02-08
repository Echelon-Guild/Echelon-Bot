using Azure;
using Azure.Data.Tables;

namespace EchelonBot.Models.Entities
{
    public class WoWInstanceInfoEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public bool Legacy { get; set; }
        public InstanceType InstanceType { get; set; }

        public ETag ETag { get; set; }
    }
}