using Azure.Data.Tables;
using Discord;
using Discord.Interactions;
using EchelonBot.Models;
using EchelonBot.Models.Entities;
using EchelonBot.Models.Modals;

namespace EchelonBot.Modules
{
    public class TeamModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly TableClient _teamTable;

        private static Dictionary<Guid, NewTeamRequest> _workingCache = new();

        public TeamModule(TableServiceClient tableServiceClient)
        {
            _teamTable = tableServiceClient.GetTableClient(TableNames.TEAM_TABLE_NAME);
            _teamTable.CreateIfNotExists();
        }

        [SlashCommand("newteam", "Create a new Team")]
        public async Task NewTeam(InstanceType instanceType)
        {
            Guid id = Guid.NewGuid();

            var request = new NewTeamRequest()
            {
                Id = id,
                ForInstanceType = instanceType
            };

            _workingCache.Add(id, request);

            await RespondWithModalAsync<NewTeamModal>($"newteam_{id}");
        }

        [ModalInteraction("newteam_*")]
        public async Task HandleNewTeam(string customId, NewTeamModal modal)
        {
            Guid id = Guid.Parse(customId);

            try
            {
                if (_teamTable.Query<WoWTeamEntity>(e => e.Name == modal.Name).Any())
                {
                    await RespondAsync($"Sorry! Team name {modal.Name} is taken! You'll have to pick another one.");
                    return;
                }

                InstanceType type = _workingCache[id].ForInstanceType;

                var newTeam = new WoWTeamEntity()
                {
                    PartitionKey = type.ToString(),
                    RowKey = id.ToString(),
                    Name = modal.Name,
                    DisplayName = modal.Name,
                    Description = modal.Description,
                    ForInstanceType = type
                };

                await _teamTable.UpsertEntityAsync(newTeam);

                await RespondAsync($"{newTeam.Name} created!", ephemeral: true);
            }
            finally
            {
                _workingCache.Remove(id);
            }
        }

        [SlashCommand("listteams", "List the available teams")]
        public async Task ListTeams(InstanceType instanceType)
        {
            var entities = _teamTable.Query<WoWTeamEntity>(e => e.ForInstanceType == instanceType);

            if (entities.Any())
            {

            }
            else
            {
                await RespondAsync($"No {instanceType} teams found. You should make one!");
            }
        }
    }
}
