using Discord.Interactions;
using EchelonBot.Models.WoW;
using EchelonBot.Services.WoW;
using System.Web;

namespace EchelonBot.Modules
{
    public class WoWInfoModule : InteractionModuleBase<SocketInteractionContext>
    {
        private WoWApiService _wowApiService;

        public WoWInfoModule(WoWApiService wowApiService)
        {
            _wowApiService = wowApiService;
        }

        [SlashCommand("searchmount", "Search for mounts")]
        public async Task SearchMount(string name)
        {

            await DeferAsync();

            string endpoint = $"data/wow/search/mount?namespace=static-us&name.en_US={name}&orderby=id&_page=1";

            try
            {
                MountSearchResult response = await _wowApiService.Get<MountSearchResult>(endpoint);

                if (response.Results.Count() > 0)
                    await FollowupAsync($"That worked! - First mount found was {response.Results[0].Data.Name.EnglishUS}", ephemeral: true);
                else
                    await FollowupAsync("That worked, but I didn't find a match.", ephemeral: true);
            }
            catch (Exception ex)
            {
                await FollowupAsync($"That didn't seem to work! - {ex.Message}", ephemeral: true);
            }
        }

    }
}
