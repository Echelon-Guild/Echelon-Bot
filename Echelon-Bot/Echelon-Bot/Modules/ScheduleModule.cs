using Azure.Data.Tables;
using Discord;
using Discord.Interactions;
using EchelonBot.Models;
using EchelonBot.Models.Entities;
using System.Text;

namespace EchelonBot
{
    public class ScheduleModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly TableClient _eventsTableClient;
        private readonly TableClient _attendeeTableClient;

        public ScheduleModule(TableServiceClient tableServiceClient)
        {
            _eventsTableClient = tableServiceClient.GetTableClient("EchelonEvents");
            _eventsTableClient.CreateIfNotExists();

            _attendeeTableClient = tableServiceClient.GetTableClient("AttendeeRecords");
            _attendeeTableClient.CreateIfNotExists();
        }

        public async Task SaveEventToTableStorage(ulong messageId, EchelonEvent event_)
        {
            var entity = new EchelonEventEntity
            {
                PartitionKey = event_.EventType.ToString(),
                RowKey = event_.Id.ToString(),
                EventName = event_.Name,
                EventDateTime = event_.EventDateTime,
                Organizer = Context.User.Username,
                MessageId = messageId
            };

            await _eventsTableClient.UpsertEntityAsync(entity);
        }

        public async Task SaveAttendeeRecordToTableStorage(AttendeeRecord record)
        {
            var entity = new AttendeeRecordEntity
            {
                PartitionKey = record.EventId.ToString(),
                RowKey = record.DiscordName,
                DiscordName = record.DiscordName,
                DiscordDisplayName = record.DiscordDisplayName,
                Role = record.Role,
                Class = record.Class,
                Spec = record.Spec
            };

            await _attendeeTableClient.UpsertEntityAsync(entity);
        }

        // Create a new event

        [SlashCommand("mythic", "Schedule a Mythic+")]
        public async Task Mythic(DateTime time, string name)
        {
            var event_ = NewEchelonEvent(EventType.Dungeon, time, name);
            event_.Id = GetNextAvailableEventId();

            var message = await RespondToGameEventAsync(event_);
            await SaveEventToTableStorage(message.Id, event_);
        }

        [SlashCommand("raid", "Schedule a Raid")]
        public async Task Raid(DateTime time, string name)
        {
            var event_ = NewEchelonEvent(EventType.Raid, time, name);
            event_.Id = GetNextAvailableEventId();

            var message = await RespondToGameEventAsync(event_);
            await SaveEventToTableStorage(message.Id, event_);
        }

        [SlashCommand("meeting", "Schedule a Meeting")]
        public async Task Meeting(DateTime time, string name)
        {
            var event_ = NewEchelonEvent(EventType.Meeting, time, name);
            event_.Id = GetNextAvailableEventId();

            var message = await RespondToMeetingEventAsync(event_);
            await SaveEventToTableStorage(message.Id, event_);
        }


        private EchelonEvent NewEchelonEvent(EventType eventType, DateTime time, string name)
        {
            var event_ = new EchelonEvent(name, time.ToUniversalTime(), eventType);

            return event_;
        }

        private Embed CreateEmbed(EchelonEvent ecEvent, IEnumerable<AttendeeRecord> attendees = null)
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

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle(ecEvent.Name)
                .WithDescription($"This is a {ecEvent.EventType.ToString()} event, scheduled for {ecEvent.EventDateTime.ToUniversalTime()}.")
                .WithColor(color)
                .AddField("Event Type", ecEvent.EventType.ToString(), true)
                .AddField("Organizer", Context.User.GlobalName, true)
                .WithThumbnailUrl(Context.User.GetAvatarUrl())
                .WithFooter("Powered by Frenzied Regeneration");
                
            if (attendees != null)
            {
                embed.AddField("Attendees", GetAttendeeString(attendees, ecEvent.EventType));
            }

            return embed.Build();
        }

        private string GetAttendeeString(IEnumerable<AttendeeRecord> attendees, EventType eventType)
        {
            if (!attendees.Any())
                return string.Empty;

            StringBuilder sb = new();

            foreach (AttendeeRecord attendee in attendees)
            {
                if (eventType == EventType.Meeting)
                    sb.AppendLine($"{attendee.Role} - {attendee.DiscordDisplayName}");
                else
                {
                    StringBuilder sb2 = new();
                    sb2.Append($"{attendee.Role} - {attendee.DiscordDisplayName}");

                    if (!string.IsNullOrWhiteSpace(attendee.Class))
                    {
                        sb2.Append($" - {attendee.Class}");
                    }

                    if (!string.IsNullOrWhiteSpace(attendee.Spec))
                    {
                        sb2.Append($" - {attendee.Spec}");
                    }

                    sb.AppendLine(sb2.ToString());
                }
                    
            }

            return sb.ToString();
        }

        private async Task<IUserMessage> RespondToGameEventAsync(EchelonEvent ecEvent)
        {
            await DeferAsync(); // Defers the response to avoid immediate timeout

            MessageComponent components = new ComponentBuilder()
                .WithButton("Sign Up", $"signup_event_{ecEvent.Id}")
                .WithButton("Absence", $"absence_event_{ecEvent.Id}")
                .WithButton("Tentative", $"tentative_event_{ecEvent.Id}")
                .Build();

            Embed embed = CreateEmbed(ecEvent);

            var message = await FollowupAsync(embed: embed, components: components); // Sends the actual message
            return message;
        }

        private async Task<IUserMessage> RespondToMeetingEventAsync(EchelonEvent ecEvent)
        {
            await DeferAsync(); // Defer the response first

            MessageComponent components = new ComponentBuilder()
                .WithButton("Sign Up", $"signupmeeting_event_{ecEvent.Id}")
                .WithButton("Absence", $"absence_event_{ecEvent.Id}")
                .WithButton("Tentative", $"tentative_event_{ecEvent.Id}")
                .Build();

            Embed embed = CreateEmbed(ecEvent);

            var message = await FollowupAsync(embed: embed, components: components);
            return message;
        }


        private int GetNextAvailableEventId()
        {
            return Random.Shared.Next();

        }

        public async Task UpdateEventEmbed(int eventId)
        {
            // Retrieve event entity (including MessageId)
            var eventEntity = _eventsTableClient.Query<EchelonEventEntity>()
                .FirstOrDefault(e => e.RowKey == eventId.ToString());

            if (eventEntity == null)
            {
                await RespondAsync("Event not found or missing message ID.");
                return;
            }

            // Retrieve the Discord message
            var channel = Context.Client.GetChannel(Context.Channel.Id) as IMessageChannel;
            var message = await channel.GetMessageAsync(eventEntity.MessageId) as IUserMessage;

            if (message == null)
            {
                await RespondAsync("Message not found.");
                return;
            }

            // Fetch all attendees from Azure Table Storage
            var attendees = _attendeeTableClient.Query<AttendeeRecordEntity>()
                .Where(a => a.PartitionKey == eventId.ToString())
                .Select(a => new AttendeeRecord(eventId, a.DiscordName, a.DiscordDisplayName, a.Role, a.Class, a.Spec))
                .ToList();

            // Rebuild the embed with updated attendees
            var updatedEvent = new EchelonEvent(eventEntity.EventName, eventEntity.EventDateTime, Enum.Parse<EventType>(eventEntity.PartitionKey));
            var embed = CreateEmbed(updatedEvent, attendees);

            // Modify the existing message with the updated embed
            await message.ModifyAsync(msg => msg.Embed = embed);
        }


        // Record response to a meeting

        [ComponentInteraction("signupmeeting_*")]
        public async Task HandleMeetingSignup(string customId)
        {
            int eventId = int.Parse(customId.Split('_')[1]);

            AttendeeRecord record = new AttendeeRecord(eventId, Context.User.Username, Context.User.GlobalName, "Attendee");

            int recordId = GetNextAvailableAttendeeRecordId();

            record.Id = recordId;

            SaveAttendeeRecordToTableStorage(record).Wait();

            await UpdateEventEmbed(eventId);

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

            AttendeeRecord record = new(eventId, Context.User.Username, Context.User.GlobalName, role, selectedClass, selectedSpec);

            SaveAttendeeRecordToTableStorage(record).Wait();

            await UpdateEventEmbed(eventId);

            // Confirm signup
            await RespondAsync($"✅ {user} signed up as a **{selectedSpec.Replace("_", " ").ToUpper()} {selectedClass.ToUpper()}** ({role})", ephemeral: true);
        }

        [ComponentInteraction("absence_event_*")]
        public async Task HandleAbscence(string customId)
        {
            int eventId = int.Parse(customId);

            AttendeeRecord record = new(eventId, Context.User.Username, Context.User.GlobalName, "Absent", string.Empty, string.Empty);

            SaveAttendeeRecordToTableStorage(record).Wait();

            await UpdateEventEmbed(eventId);

            await RespondAsync("We'll miss you!", ephemeral: true);
        }

        [ComponentInteraction("tentative_event_*")]
        public async Task HandleTentative(string customId)
        {
            int eventId = int.Parse(customId);

            AttendeeRecord record = new(eventId, Context.User.Username, Context.User.GlobalName, "Tentative", string.Empty, string.Empty);

            SaveAttendeeRecordToTableStorage(record).Wait();

            await UpdateEventEmbed(eventId);

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

        private int GetNextAvailableAttendeeRecordId()
        {
            return Random.Shared.Next();

        }
    }
}