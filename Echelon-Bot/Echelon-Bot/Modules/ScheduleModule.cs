using Discord;
using Discord.Interactions;
using EchelonBot.Data;
using EchelonBot.Models;

namespace EchelonBot
{
    public class ScheduleModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly EchelonBotDbContext _db;

        public ScheduleModule(EchelonBotDbContext echelonBotDbContext)
        {
            _db = echelonBotDbContext;
        }

        [SlashCommand("mythic", "Schedule a Mythic+")]
        public Task Mythic(DateTime time, string name)
        {
            EchelonEvent event_ = NewEchelonEvent(EventType.Dungeon, time, name);

            return RespondToGameEventAsync(event_);
        }

        [SlashCommand("raid", "Schedule a Raid")]
        public Task Raid(DateTime time, string name)
        {
            EchelonEvent event_ = NewEchelonEvent(EventType.Raid, time, name);

            return RespondToGameEventAsync(event_);
        }

        [SlashCommand("meeting", "Schedule a Meeting")]
        public Task Meeting(DateTime time, string name)
        {
            EchelonEvent event_ = NewEchelonEvent(EventType.Meeting, time, name);

            return RespondToMeetingEventAsync(event_);
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

        private Task RespondToGameEventAsync(EchelonEvent ecEvent)
        {
            MessageComponent components = new ComponentBuilder()
                .WithButton("Sign Up", $"signup_event_{ecEvent.Id}")
                .WithButton("Abscence", $"abscence_event_{ecEvent.Id}")
                .WithButton("Tentative", $"tentative_event_{ecEvent.Id}")
                .Build();

            Embed embed = CreateEmbed(ecEvent);

            return RespondAsync(embed: embed, components: components);
        }

        private Task RespondToMeetingEventAsync(EchelonEvent ecEvent)
        {
            MessageComponent components = new ComponentBuilder()
                .WithButton("Sign Up", $"signupmeeting_event_{ecEvent.Id}")
                .WithButton("Abscence", $"abscence_event_{ecEvent.Id}")
                .WithButton("Tentative", $"tentative_event_{ecEvent.Id}")
                .Build();

            Embed embed = CreateEmbed(ecEvent);

            return RespondAsync(embed: embed, components: components);
        }

        [ComponentInteraction("signupmeeting_*")]
        public async Task HandleMeetingSignup(string customId)
        {
            int eventId = int.Parse(customId.Split('_')[1]);

            await RespondAsync("See you at the meeting!", ephemeral: true);
        }

        [ComponentInteraction("signup_*")]
        public async Task HandleSignup(string customId)
        {
            int eventId = int.Parse(customId.Split('_')[1]);

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
            int eventId = int.Parse(customId);

            var specDropdown = new SelectMenuBuilder()
                .WithCustomId($"spec_select_{eventId}_{selectedClass}")
                .WithPlaceholder("Select your Specialization");

            // Add relevant specs based on selected class
            switch (selectedClass)
            {
                case "death_knight":
                    specDropdown.AddOption("Blood", "blood")
                                .AddOption("Frost", "frost")
                                .AddOption("Unholy", "unholy");
                    break;
                case "demon_hunter":
                    specDropdown.AddOption("Havoc", "havoc")
                                .AddOption("Vengeance", "vengeance");
                    break;
                case "druid":
                    specDropdown.AddOption("Balance", "balance")
                                .AddOption("Feral", "feral")
                                .AddOption("Guardian", "guardian")
                                .AddOption("Restoration", "restoration");
                    break;
                case "evoker":
                    specDropdown.AddOption("Devastation", "devastation")
                                .AddOption("Preservation", "preservation")
                                .AddOption("Augmentation", "augmentation");
                    break;
                case "hunter":
                    specDropdown.AddOption("Beast Mastery", "beast_mastery")
                                .AddOption("Marksmanship", "marksmanship")
                                .AddOption("Survival", "survival");
                    break;
                case "mage":
                    specDropdown.AddOption("Arcane", "arcane")
                                .AddOption("Fire", "fire")
                                .AddOption("Frost", "frost");
                    break;
                case "monk":
                    specDropdown.AddOption("Brewmaster", "brewmaster")
                                .AddOption("Mistweaver", "mistweaver")
                                .AddOption("Windwalker", "windwalker");
                    break;
                case "paladin":
                    specDropdown.AddOption("Holy", "holy")
                                .AddOption("Protection", "protection")
                                .AddOption("Retribution", "retribution");
                    break;
                case "priest":
                    specDropdown.AddOption("Discipline", "discipline")
                                .AddOption("Holy", "holy")
                                .AddOption("Shadow", "shadow");
                    break;
                case "rogue":
                    specDropdown.AddOption("Assassination", "assassination")
                                .AddOption("Outlaw", "outlaw")
                                .AddOption("Subtlety", "subtlety");
                    break;
                case "shaman":
                    specDropdown.AddOption("Elemental", "elemental")
                                .AddOption("Enhancement", "enhancement")
                                .AddOption("Restoration", "restoration");
                    break;
                case "warlock":
                    specDropdown.AddOption("Affliction", "affliction")
                                .AddOption("Demonology", "demonology")
                                .AddOption("Destruction", "destruction");
                    break;
                case "warrior":
                    specDropdown.AddOption("Arms", "arms")
                                .AddOption("Fury", "fury")
                                .AddOption("Protection", "protection");
                    break;
            }


            var builder = new ComponentBuilder().WithSelectMenu(specDropdown);

            await RespondAsync($"You selected **{selectedClass.ToUpper()}**. Now pick your **Specialization**:", components: builder.Build(), ephemeral: true);
        }

        [ComponentInteraction("spec_select_*_*")]
        public async Task HandleSpecSelection(string customId, string selectedClass, string selectedSpec)
        {
            if (string.IsNullOrWhiteSpace(selectedSpec))
            {
                await RespondAsync("❌ No specialization selected.", ephemeral: true);
                return;
            }

            int eventId = int.Parse(customId);

            var role = GetRole(selectedClass, selectedSpec);
            var user = Context.User.GlobalName;

            // Confirm signup
            await RespondAsync($"✅ {user} signed up as a **{selectedSpec.Replace("_", " ").ToUpper()} {selectedClass.ToUpper()}** ({role})", ephemeral: true);

            // Update event embed
            //await UpdateEventEmbed(eventId);
        }

        [ComponentInteraction("abscence_event_*")]
        public async Task HandleAbscence(string customId)
        {
            int eventId = int.Parse(customId);

            await RespondAsync("We'll miss you!", ephemeral: true);
        }

        [ComponentInteraction("tentative_event_*")]
        public async Task HandleTentative(string customId)
        {
            int eventId = int.Parse(customId);

            await RespondAsync("We hope to see you!", ephemeral: true);
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