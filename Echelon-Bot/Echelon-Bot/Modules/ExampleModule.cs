using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace EchelonBot
{
    public class ExampleModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("say", "Make the bot say something.")]
        public Task Say(string text)
            => RespondAsync(text);
    }
}