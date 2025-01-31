using Discord.Interactions;

namespace EchelonBot
{
    public class ScheduleModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("mythic", "Schedule a Mythic+")]
        public Task Mythic(DateTime time, string name)
        {
            return RespondAsync($"Raid {name} scheduled for {time}.");
        }

        [SlashCommand("raid", "Schedule a Raid")]
        public Task Raid(DateTime time, string name)
        {
            return RespondAsync($"Raid {name} scheduled for {time}.");
        }
    }
}