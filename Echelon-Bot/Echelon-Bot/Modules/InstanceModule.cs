using Azure.Data.Tables;
using Discord;
using Discord.Interactions;
using EchelonBot.Models.Entities;
using EchelonBot.Models.WoW;
using System.Collections.Generic;
using System.Text;

namespace EchelonBot.Modules
{
    public class InstanceModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly TableClient _instanceTable;
        private readonly EmbedFactory _embedFactory;

        private static readonly Dictionary<Guid, WoWInstanceInfoEntity> _workingCache = new();

        public InstanceModule(TableServiceClient tableServiceClient, EmbedFactory embedFactory)
        {
            _instanceTable = tableServiceClient.GetTableClient("Instances");
            _instanceTable.CreateIfNotExists();

            _embedFactory = embedFactory;
        }

        [SlashCommand("newinstance", "Add a new instance to the database")]
        public async Task NewInstance()
        {
            Guid id = Guid.NewGuid();

            var entity = new WoWInstanceInfoEntity()
            {
                RowKey = id.ToString(),
            };

            _workingCache.Add(id, entity);

            var mb = new ModalBuilder()
                .WithTitle("Add Instance to database")
                .WithCustomId($"newinstance_{id}")
                .AddTextInput("Name", "instanceName")
                .AddTextInput("Type", "instanceType")
                .AddTextInput("Is this legacy? Say True or False", "instanceLegacy")
                .AddTextInput("Image Url for the instance", "instanceUrl");

            await RespondWithModalAsync(mb.Build());
        }

        [ModalInteraction("newinstance_*")]
        public async Task HandleNewInstance(string customId, NewInstanceModal modal)
        {
            Guid id = Guid.Parse(customId); // Extract ID from custom ID

            if (Enum.TryParse(modal.InstanceType, out InstanceType _instanceType) &&
                bool.TryParse(modal.InstanceLegacy, out bool _instanceLegacy))
            {
                _workingCache[id].Name = modal.InstanceName;
                _workingCache[id].PartitionKey = _instanceType.ToString();
                _workingCache[id].Legacy = _instanceLegacy;
                _workingCache[id].ImageUrl = modal.InstanceUrl;
                _workingCache[id].InstanceType = _instanceType;
            }
            else
            {
                await RespondAsync("Check your input for Type and Legacy. Use 'Dungeon' or 'Raid' for Type and 'True' or 'False' for Legacy.");
                return;
            }

            await _instanceTable.UpsertEntityAsync(_workingCache[id]);
            await RespondAsync($"Instance {modal.InstanceName} saved.");
        }


        [SlashCommand("listinstances", "List the stored instances")]
        public async Task ListInstances()
        {
            var entities = _instanceTable.Query<WoWInstanceInfoEntity>(e => true);

            var dungeons = entities.Where(e => e.InstanceType == InstanceType.Dungeon);
            var raids = entities.Where(e => e.InstanceType == InstanceType.Raid);

            bool hasRaids = raids.Any();
            bool hasDungeons = dungeons.Any();

            if (!hasRaids && !hasDungeons)
            {
                await RespondAsync("No instances currently stored", ephemeral: true);
                return;
            }

            List<Embed> embeds = new();

            if (raids.Any())
            {   
                foreach (WoWInstanceInfoEntity raid in raids)
                {
                    embeds.Add(_embedFactory.CreateInstanceEmbed(raid));    
                }
            }

            if (dungeons.Any())
            {
                foreach (WoWInstanceInfoEntity dungeon in dungeons)
                {
                    embeds.Add(_embedFactory.CreateInstanceEmbed(dungeon));
                }
            }

            await RespondAsync(embeds: embeds.ToArray(), ephemeral: true);
        }

        [SlashCommand("deleteinstance", "Remove a stored instance.")]
        public async Task DeleteInstance(string id)
        {
            if (Guid.TryParse(id, out Guid _id))
            {
                var areYouSureBuilder = new SelectMenuBuilder()
                    .WithCustomId($"deleteconfirmed_{id}")
                    .WithPlaceholder("Are you sure you want to delete this instance?")
                    .AddOption("Yes", "yes")
                    .AddOption("No", "no");

                var builder = new ComponentBuilder().WithSelectMenu(areYouSureBuilder);

                await RespondAsync("Are you sure you want to delete this instance?", components: builder.Build(), ephemeral: true);

                return;
            }

            await RespondAsync($"{id} not found, make sure you are using the ID and not the name.", ephemeral: true);
        }

        [ComponentInteraction("deleteconfirmed_*")]
        public async Task DeleteConfirmed(string id, string confirmed)
        {
            Guid _id = Guid.Parse(id);

            if (confirmed.ToLower() == "yes")
            {
                var entity = _instanceTable.Query<WoWInstanceInfoEntity>(e => e.RowKey == id).First();

                await _instanceTable.DeleteEntityAsync(entity);

                await RespondAsync($"Instance {id} removed.", ephemeral: true);

                return;
            }

            await RespondAsync("Deletion abandoned", ephemeral: true);
        }
    }
}
