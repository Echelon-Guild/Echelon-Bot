using Azure.Data.Tables;
using Discord;
using Discord.Interactions;
using EchelonBot.Models.Entities;
using EchelonBot.Models.Modals;
using EchelonBot.Models.WoW;
using EchelonBot.Services;

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

            await RespondWithModalAsync<NewInstanceModal>($"newinstance_{id}");
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

            bool nameTaken = _instanceTable.Query<WoWInstanceInfoEntity>(e => e.Name.ToLower() == modal.InstanceType.ToLower()).Any();

            if (nameTaken) 
            {
                await RespondAsync($"Name {modal.InstanceName} is already taken. You must delete it before uploading again.");
                return;
            }

            await _instanceTable.UpsertEntityAsync(_workingCache[id]);
            await RespondAsync($"Instance {modal.InstanceName} saved.");
        }


        [SlashCommand("listinstances", "List the stored instances")]
        public async Task ListInstances()
        {
            var entities = _instanceTable.Query<WoWInstanceInfoEntity>(e => true);

            Embed embed = _embedFactory.CreateInstanceEmbed(entities);

            await RespondAsync(embed: embed, ephemeral: true);
        }

        [SlashCommand("getinstance", "Get a specific instance by name or ID")]
        public async Task GetInstance(string identifier)
        {
            WoWInstanceInfoEntity entity;

            if (Guid.TryParse(identifier, out Guid id))
            {
                entity = _instanceTable.Query<WoWInstanceInfoEntity>(e => e.RowKey == id.ToString()).First();
            }
            else
            {
                entity = _instanceTable.Query<WoWInstanceInfoEntity>(e => e.Name == identifier).First();
            }

            Embed embed = _embedFactory.CreateInstanceEmbed(entity);

            await RespondAsync(embed: embed, ephemeral: true);
        }

        [SlashCommand("deleteinstance", "Remove a stored instance.")]
        public async Task DeleteInstance(string id)
        {
            if (Guid.TryParse(id, out Guid _id))
            {
                WoWInstanceInfoEntity entity = _instanceTable.Query<WoWInstanceInfoEntity>(e => e.RowKey == _id.ToString()).First();

                var areYouSureBuilder = new SelectMenuBuilder()
                    .WithCustomId($"deleteconfirmed_{id}")
                    .WithPlaceholder($"Are you sure you want to delete {entity.Name}?")
                    .AddOption("Yes", "yes")
                    .AddOption("No", "no");

                var builder = new ComponentBuilder().WithSelectMenu(areYouSureBuilder);

                await RespondAsync($"Are you sure you want to delete {entity.Name}", components: builder.Build(), ephemeral: true);

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
