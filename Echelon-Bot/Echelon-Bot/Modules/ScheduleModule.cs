using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace EchelonBot
{
    public class ScheduleModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("mythic", "Schedule a Mythic+")]
        public Task Mythic(string text)
            => RespondAsync(text);

        [SlashCommand("raid", "Schedule a Raid")]
        public Task Raid(string text)
            => RespondAsync(text);
    }
}