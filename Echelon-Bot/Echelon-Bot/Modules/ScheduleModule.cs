using Discord;
using Discord.Interactions;
using EchelonBot.Models;

namespace EchelonBot
{
    public class ScheduleModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("mythic", "Schedule a Mythic+")]
        public Task Mythic(DateTime time, string name)
        {
            EchelonEvent event_ = NewEchelonEvent(EventType.Dungeon, time, name);

            return RespondAsync(embed: CreateEmbed(event_));
        }

        [SlashCommand("raid", "Schedule a Raid")]
        public Task Raid(DateTime time, string name)
        {
            EchelonEvent event_ = NewEchelonEvent(EventType.Raid, time, name);

            return RespondAsync(embed: CreateEmbed(event_));
        }

        [SlashCommand("meeting", "Schedule a Meeting")]
        public Task Meeting(DateTime time, string name)
        {
            EchelonEvent event_ = NewEchelonEvent(EventType.Meeting, time, name);

            return RespondAsync(embed: CreateEmbed(event_));
        }

        private EchelonEvent NewEchelonEvent(EventType eventType, DateTime time, string name)
        {
            int id = GetNextAvailableId();

            var event_ = new EchelonEvent(id, name, time.ToUniversalTime(), eventType);

            return event_;
        }

        private int GetNextAvailableId()
        {
            //TODO: Replace this with something that can actually create sequential event ids from the DB.
            return Random.Shared.Next();
        }

        private Embed CreateEmbed(EchelonEvent ecEvent)
        {
            Color color;

            switch (ecEvent.EventType)
            {
                case EventType.Raid:
                {
                    color = Color.Orange;
                    break;
                }
                case EventType.Dungeon:
                {
                    color = Color.Green;
                    break;
                }
                case EventType.Meeting:
                {
                    color = Color.Blue;
                    break;
                }
                default:
                {
                    color = Color.Red;
                    break;
                }
            }

            Embed embed = new EmbedBuilder()
                .WithTitle(ecEvent.Name)
                .WithDescription($"This is a {ecEvent.EventType.ToString()} event, scheduled for {ecEvent.EventDateTime.ToUniversalTime()}.")
                .WithColor(color)
                .AddField("Event Type", ecEvent.EventType.ToString(), true)
                .AddField("Organizer", Context.User.GlobalName, true)
                .WithThumbnailUrl(Context.User.GetAvatarUrl())
                .WithFooter("Powered by Frenzied Regeneration")
                .Build();

            return embed;
        }
    }
}