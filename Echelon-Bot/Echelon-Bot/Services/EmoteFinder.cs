using Azure.Data.Tables;
using EchelonBot.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchelonBot.Services
{
    public class EmoteFinder
    {
        private TableClient _storedEmotes;

        public EmoteFinder(TableServiceClient tableServiceClient) 
        {
            _storedEmotes = tableServiceClient.GetTableClient("StoredEmotes");
            _storedEmotes.CreateIfNotExists();
        }

        public string GetEmote(string className, string spec)
        {
            StoredEmoteEntity entity = _storedEmotes.Query<StoredEmoteEntity>(e => e.PartitionKey == className.ToLower() && e.RowKey == spec.ToLower()).First();

            return entity.EmoteID ?? "❌";
        }
    }
}
