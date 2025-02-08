using Azure;
using Azure.Data.Tables;

namespace EchelonBot.Models.Entities
{
    public class WoWTeamEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public InstanceType ForInstanceType { get; set; }

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
