using Discord.Interactions;
using EchelonBot.Models.WoW;
using EchelonBot.Services.WoW;

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

            Uri uri = new Uri($"https://us.api.blizzard.com/data/wow/search/mount?namespace=static-us&name.en_US=Turtle&orderby=id&_page=1");

            try
            {
                var response = await _wowApiService.Get<MountSearchResult>(uri);

                await FollowupAsync("That worked!", ephemeral: true);
            }
            catch (Exception ex)
            {
                await FollowupAsync($"That didn't seem to work! - {ex.Message}", ephemeral: true);
            }
        }

    }
}
