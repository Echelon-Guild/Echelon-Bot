using Discord;
using Discord.Interactions;
using EchelonBot.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace EchelonBot
{
    public class ScheduleModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("mythic", "Schedule a Mythic+")]
        public Task Mythic(DateTime time, string name)
        {
            EchelonEvent event_ = NewEchelonEvent(EventType.Dungeon, time, name);

            return RespondToEventAsync(event_);
        }

        [SlashCommand("raid", "Schedule a Raid")]
        public Task Raid(DateTime time, string name)
        {
            EchelonEvent event_ = NewEchelonEvent(EventType.Raid, time, name);

            return RespondToEventAsync(event_);
        }

        [SlashCommand("meeting", "Schedule a Meeting")]
        public Task Meeting(DateTime time, string name)
        {
            EchelonEvent event_ = NewEchelonEvent(EventType.Meeting, time, name);

            return RespondToEventAsync(event_);
        }

        private EchelonEvent NewEchelonEvent(EventType eventType, DateTime time, string name)
        {
            int id = GetNextAvailableId();

            var event_ = new EchelonEvent(id, name, time.ToUniversalTime(), eventType);

            return event_;
        }

        private int GetNextAvailableId()
        {
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

        private Task RespondToEventAsync(EchelonEvent ecEvent)
        {
            MessageComponent buttonComponent = new ComponentBuilder()
                .WithButton("Sign Up", $"signup_event_{ecEvent.Id}")
                .Build();

            Embed embed = CreateEmbed(ecEvent);

            return RespondAsync(embed: embed, components: buttonComponent);
        }

        [ComponentInteraction("signup_*")]
        public async Task HandleSignup(string customId)
        {
            var eventId = int.Parse(customId.Split('_')[1]);

            var classDropdown = new SelectMenuBuilder()
                .WithCustomId($"class_select_{eventId}")
                .WithPlaceholder("Select your Class")
                .AddOption("Death Knight", "death_knight")
                .AddOption("Demon Hunter", "demon_hunter")
                .AddOption("Druid", "druid")
                .AddOption("Evoker", "evoker")
                .AddOption("Hunter", "hunter")
                .AddOption("Mage", "mage")
                .AddOption("Monk", "monk")
                .AddOption("Paladin", "paladin")
                .AddOption("Priest", "priest")
                .AddOption("Rogue", "rogue")
                .AddOption("Shaman", "shaman")
                .AddOption("Warlock", "warlock")
                .AddOption("Warrior", "warrior");

            var builder = new ComponentBuilder().WithSelectMenu(classDropdown);

            await RespondAsync("Select your **Class**:", components: builder.Build(), ephemeral: true);
        }

        [ComponentInteraction("class_select_*")]
        public async Task HandleClassSelection(string customId, string selectedClass)
        {
            var eventId = int.Parse(customId);

            var specDropdown = new SelectMenuBuilder()
                .WithCustomId($"spec_select_{eventId}_{selectedClass}")
                .WithPlaceholder("Select your Specialization");

            // Add relevant specs based on selected class
            switch (selectedClass)
            {
                case "death_knight":
                    specDropdown.AddOption("Blood", "death_knight-blood").AddOption("Frost", "death_knight-frost").AddOption("Unholy", "death_knight-unholy");
                    break;
                case "demon_hunter":
                    specDropdown.AddOption("Havoc", "demon_hunter-havoc").AddOption("Vengeance", "demon_hunter-vengeance");
                    break;
                case "druid":
                    specDropdown.AddOption("Balance", "druid-balance").AddOption("Feral", "druid-feral")
                                 .AddOption("Guardian", "guardian").AddOption("Restoration", "restoration");
                    break;
                case "evoker":
                    specDropdown.AddOption("Devastation", "evoker-devastation").AddOption("Preservation", "evoker-preservation")
                                 .AddOption("Augmentation", "augmentation");
                    break;
                case "hunter":
                    specDropdown.AddOption("Beast Mastery", "hunter-beast_mastery").AddOption("Marksmanship", "hunter-marksmanship")
                                 .AddOption("Survival", "survival");
                    break;
                case "mage":
                    specDropdown.AddOption("Arcane", "mage-arcane").AddOption("Fire", "mage-fire").AddOption("Frost", "mage-frost");
                    break;
                case "monk":
                    specDropdown.AddOption("Brewmaster", "monk-brewmaster").AddOption("Mistweaver", "monk-mistweaver")
                                 .AddOption("Windwalker", "monk-windwalker");
                    break;
                case "paladin":
                    specDropdown.AddOption("Holy", "paladin-holy").AddOption("Protection", "paladin-protection").AddOption("Retribution", "paladin-retribution");
                    break;
                case "priest":
                    specDropdown.AddOption("Discipline", "priest-discipline").AddOption("Holy", "priest-holy").AddOption("Shadow", "priest-shadow");
                    break;
                case "rogue":
                    specDropdown.AddOption("Assassination", "rogue-assassination").AddOption("Outlaw", "rogue-outlaw").AddOption("Subtlety", "rogue-subtlety");
                    break;
                case "shaman":
                    specDropdown.AddOption("Elemental", "shaman-elemental").AddOption("Enhancement", "shaman-enhancement")
                                 .AddOption("Restoration", "shaman-restoration");
                    break;
                case "warlock":
                    specDropdown.AddOption("Affliction", "warlock-affliction").AddOption("Demonology", "warlock-demonology")
                                 .AddOption("Destruction", "warlock-destruction");
                    break;
                case "warrior":
                    specDropdown.AddOption("Arms", "warrior-arms").AddOption("Fury", "warrior-fury").AddOption("Protection", "warrior-protection");
                    break;
            }

            var builder = new ComponentBuilder().WithSelectMenu(specDropdown);

            await RespondAsync($"You selected **{selectedClass.ToUpper()}**. Now pick your **Specialization**:", components: builder.Build(), ephemeral: true);
        }

        [ComponentInteraction("spec_select_*_*")]
        public async Task HandleSpecSelection(string customId, string selectedSpec)
        {
            Console.WriteLine(customId);

            if (string.IsNullOrWhiteSpace(selectedSpec))
            {
                await RespondAsync("❌ No specialization selected.", ephemeral: true);
                return;
            }

            var parts = customId.Split('_');
            if (parts.Length < 4)
            {
                await RespondAsync("❌ Invalid selection data.", ephemeral: true);
                return;
            }

            var eventId = int.Parse(parts[2]);
            var selectedClass = parts[3];

            var role = GetRole(selectedClass, selectedSpec);
            var user = Context.User.Username;

            // Confirm signup
            await RespondAsync($"✅ {user} signed up as a **{selectedSpec.ToUpper()} {selectedClass.ToUpper()}** ({role})", ephemeral: true);

            // Update event embed
            //await UpdateEventEmbed(eventId);
        }


        private string GetRole(string playerClass, string spec)
        {
            var tanks = new HashSet<string> { "Blood Death Knight", "Guardian Druid", "Brewmaster Monk", "Protection Paladin", "Protection Warrior", "Vengeance Demon Hunter" };
            var healers = new HashSet<string> { "Restoration Druid", "Mistweaver Monk", "Holy Paladin", "Holy Priest", "Discipline Priest", "Restoration Shaman", "Preservation Evoker" };

            string fullSpec = $"{spec.FirstCharToUpper()} {playerClass.FirstCharToUpper()}";

            if (tanks.Contains(fullSpec)) return "Tank";
            if (healers.Contains(fullSpec)) return "Healer";
            return "DPS";
        }


    }
}