using Azure.Data.Tables;
using Discord;
using Discord.Interactions;
using EchelonBot.Models;
using EchelonBot.Models.Entities;
using EchelonBot.Services;

namespace EchelonBot.Modules
{
    public class ScheduleModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly TableClient _eventsTableClient;
        private readonly TableClient _attendeeTableClient;
        private readonly TableClient _scheduledMessageTableClient;

        private readonly EmbedFactory _embedFactory;

        private int[] _longMonths = [1, 3, 5, 7, 8, 10, 12];

        private static Dictionary<int, ScheduleEventRequest> _requestWorkingCache = new Dictionary<int, ScheduleEventRequest>();

        public ScheduleModule(TableServiceClient tableServiceClient, EmbedFactory embedFactory)
        {
            _eventsTableClient = tableServiceClient.GetTableClient(TableNames.EVENT_TABLE_NAME);
            _eventsTableClient.CreateIfNotExists();

            _attendeeTableClient = tableServiceClient.GetTableClient(TableNames.ATTENDEE_TABLE_NAME);
            _attendeeTableClient.CreateIfNotExists();

            _scheduledMessageTableClient = tableServiceClient.GetTableClient(TableNames.SCHEDULED_MESSAGE_TABLE_NAME);
            _scheduledMessageTableClient.CreateIfNotExists();

            _embedFactory = embedFactory;
        }

        public async Task SaveEventToTableStorage(ulong messageId, EchelonEvent event_)
        {
            var entity = new EchelonEventEntity
            {
                PartitionKey = event_.EventType.ToString(),
                RowKey = event_.Id.ToString(),
                EventName = event_.Name,
                EventDateTime = event_.EventDateTime,
                EventDescription = event_.Description,
                Organizer = event_.Organizer,
                ImageUrl = event_.ImageUrl,
                Footer = event_.Footer,
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

        // The below is a chain. You trigger it with a slash command then respond to the dropdowns until you reach the end
        // I tried to keep things in order.

        [SlashCommand("mythic", "Schedule a Mythic+")]
        public async Task Mythic(string name, string description)
        {
            ScheduleEventRequest request = new();

            request.Name = name;
            request.Id = GetNextAvailableEventId();
            request.EventType = EventType.Dungeon;
            request.Description = description;

            _requestWorkingCache.Add(request.Id, request);

            await RespondToTypeSelected(request.Id);
        }

        [SlashCommand("raid", "Schedule a Raid")]
        public async Task Raid(string name, string description)
        {
            ScheduleEventRequest request = new();

            request.Name = name;
            request.Id = GetNextAvailableEventId();
            request.EventType = EventType.Raid;
            request.Description = description;

            _requestWorkingCache.Add(request.Id, request);

            await RespondToTypeSelected(request.Id);
        }

        [SlashCommand("meeting", "Schedule a Meeting")]
        public async Task Meeting(string name, string description)
        {
            ScheduleEventRequest request = new();

            request.Name = name;
            request.Id = GetNextAvailableEventId();
            request.EventType = EventType.Meeting;
            request.Description = description;

            _requestWorkingCache.Add(request.Id, request);

            await RespondToTypeSelected(request.Id);
        }

        [SlashCommand("event", "Schedule an event")]
        public async Task Event(string name, string description)
        {
            ScheduleEventRequest request = new();
            request.Name = name;
            request.Id = GetNextAvailableEventId();
            request.Description = description;

            _requestWorkingCache.Add(request.Id, request);

            var eventTypeDropdown = new SelectMenuBuilder()
                .WithCustomId($"event_select_{request.Id}")
                .WithPlaceholder("Choose an event type")
                .AddOption("Raid", EventType.Raid.ToString())
                .AddOption("Dungeon", EventType.Dungeon.ToString())
                .AddOption("Meeting", EventType.Meeting.ToString());

            var builder = new ComponentBuilder().WithSelectMenu(eventTypeDropdown);

            await RespondAsync("Choose an event type:", components: builder.Build(), ephemeral: true);
        }

        [ComponentInteraction("event_select_*")]
        public async Task HandleEventTypeSelected(string customId, string eventType)
        {
            int eventId = int.Parse(customId);

            EventType type = Enum.Parse<EventType>(eventType);

            _requestWorkingCache[eventId].EventType = type;

            await RespondToTypeSelected(eventId);
        }

        private async Task RespondToTypeSelected(int eventId)
        {
            var monthDropdown = new SelectMenuBuilder()
                .WithCustomId($"month_select_{eventId}")
                .WithPlaceholder("Select the month of the event")
                .AddOption(DateTime.Now.ToString("MMMM"), DateTime.Now.Month.ToString())
                .AddOption(DateTime.Now.AddMonths(1).ToString("MMMM"), DateTime.Now.AddMonths(1).Month.ToString())
                .AddOption(DateTime.Now.AddMonths(2).ToString("MMMM"), DateTime.Now.AddMonths(2).Month.ToString())
                .AddOption(DateTime.Now.AddMonths(3).ToString("MMMM"), DateTime.Now.AddMonths(3).Month.ToString())
                .AddOption(DateTime.Now.AddMonths(4).ToString("MMMM"), DateTime.Now.AddMonths(4).Month.ToString())
                .AddOption(DateTime.Now.AddMonths(5).ToString("MMMM"), DateTime.Now.AddMonths(5).Month.ToString())
                .AddOption(DateTime.Now.AddMonths(6).ToString("MMMM"), DateTime.Now.AddMonths(6).Month.ToString())
                .AddOption(DateTime.Now.AddMonths(7).ToString("MMMM"), DateTime.Now.AddMonths(7).Month.ToString())
                .AddOption(DateTime.Now.AddMonths(8).ToString("MMMM"), DateTime.Now.AddMonths(8).Month.ToString())
                .AddOption(DateTime.Now.AddMonths(9).ToString("MMMM"), DateTime.Now.AddMonths(9).Month.ToString())
                .AddOption(DateTime.Now.AddMonths(10).ToString("MMMM"), DateTime.Now.AddMonths(10).Month.ToString())
                .AddOption(DateTime.Now.AddMonths(11).ToString("MMMM"), DateTime.Now.AddMonths(11).Month.ToString());

            var builder = new ComponentBuilder().WithSelectMenu(monthDropdown);

            await RespondAsync("Select the month of the event:", components: builder.Build(), ephemeral: true);
        }

        [ComponentInteraction("month_select_*")]
        public async Task HandleMonthSelected(string customId, string month)
        {
            int eventId = int.Parse(customId);

            int _month = int.Parse(month);

            _requestWorkingCache[eventId].Month = _month;

            var weekDropdown = new SelectMenuBuilder()
                .WithCustomId($"week_select_{eventId}")
                .WithPlaceholder("Select the week of the event");

            AddWeekOptions(weekDropdown, _month);

            var builder = new ComponentBuilder().WithSelectMenu(weekDropdown);

            await RespondAsync("Select the week of the event:", components: builder.Build(), ephemeral: true);
        }
        private void AddWeekOptions(SelectMenuBuilder builder, int month)
        {
            builder.AddOption("Day 1-7", "1")
                .AddOption("Day 8-14", "2")
                .AddOption("Day 15-21", "3")
                .AddOption("Day 22-28", "4");

            if (_longMonths.Contains(month))
            {
                builder.AddOption("Day 29-31", "5");
            }
            else if (month != 2)
            {
                //TODO: Handle leap year
                builder.AddOption("Day 29-30", "5");
            }
        }

        [ComponentInteraction("week_select_*")]
        public async Task HandleWeekSelected(string customId, string week)
        {
            int eventId = int.Parse(customId);
            int _week = int.Parse(week);

            _requestWorkingCache[eventId].Week = _week;

            var dayDropdown = new SelectMenuBuilder()
                .WithCustomId($"day_select_{eventId}")
                .WithPlaceholder("Select the day of the event");

            AddDayOptions(dayDropdown, _requestWorkingCache[eventId].Month, _week);

            var builder = new ComponentBuilder().WithSelectMenu(dayDropdown);

            await RespondAsync("Select the day of the event:", components: builder.Build(), ephemeral: true);
        }

        private void AddDayOptions(SelectMenuBuilder builder, int month, int week)
        {
            int startingDay = 0;

            if (week == 1)
            {
                startingDay = 1;
            }
            else if (week == 2)
            {
                startingDay = 8;
            }
            else if (week == 3)
            {
                startingDay = 15;
            }
            else if (week == 4)
            {
                startingDay = 22;
            }
            else if (week == 5)
            {
                startingDay = 29;
            }
            else
            {
                throw new Exception("Something went horribly wrong. You got a week number that isn't 1-5");
            }

            int numberOfDaysInWeek = 7;

            if (week == 5)
            {
                if (_longMonths.Contains(month))
                {
                    numberOfDaysInWeek = 3;
                }
                else
                {
                    numberOfDaysInWeek = 2;
                }

                //TODO: If leap year, numberOfDaysInWeek is 1.
            }

            for (int i = 0; i < numberOfDaysInWeek; i++)
            {
                int day = startingDay + i;

                int year = DateTime.Now.Year;

                if (month < DateTime.Now.Month)
                    ++year;

                DateTime date = new DateTime(year, month, day);

                string dayString = date.ToString("dddd, MMMM dd");

                builder.AddOption(dayString, day.ToString());
            }
        }

        [ComponentInteraction("day_select_*")]
        public async Task HandleDaySelected(string customId, string day)
        {
            int eventId = int.Parse(customId);
            int _day = int.Parse(day);

            _requestWorkingCache[eventId].Day = _day;

            var hourDropdown = new SelectMenuBuilder()
                .WithCustomId($"hour_select_{eventId}")
                .WithPlaceholder("Select the hour of the event");

            AddHourOptions(hourDropdown);

            var builder = new ComponentBuilder().WithSelectMenu(hourDropdown);

            await RespondAsync("Select the hour of the event:", components: builder.Build(), ephemeral: true);
        }

        private void AddHourOptions(SelectMenuBuilder builder)
        {
            builder.AddOption("12:00 AM", "0");
            builder.AddOption("1:00 AM", "1");
            builder.AddOption("2:00 AM", "2");
            builder.AddOption("3:00 AM", "3");
            builder.AddOption("4:00 AM", "4");
            builder.AddOption("5:00 AM", "5");
            builder.AddOption("6:00 AM", "6");
            builder.AddOption("7:00 AM", "7");
            builder.AddOption("8:00 AM", "8");
            builder.AddOption("9:00 AM", "9");
            builder.AddOption("10:00 AM", "10");
            builder.AddOption("11:00 AM", "11");
            builder.AddOption("12:00 PM", "12");
            builder.AddOption("1:00 PM", "13");
            builder.AddOption("2:00 PM", "14");
            builder.AddOption("3:00 PM", "15");
            builder.AddOption("4:00 PM", "16");
            builder.AddOption("5:00 PM", "17");
            builder.AddOption("6:00 PM", "18");
            builder.AddOption("7:00 PM", "19");
            builder.AddOption("8:00 PM", "20");
            builder.AddOption("9:00 PM", "21");
            builder.AddOption("10:00 PM", "22");
            builder.AddOption("11:00 PM", "23");
        }

        [ComponentInteraction("hour_select_*")]
        public async Task HandleHourSelected(string customId, string hour)
        {
            int eventId = int.Parse(customId);
            int _hour = int.Parse(hour);

            _requestWorkingCache[eventId].Hour = _hour;

            var minuteDropdown = new SelectMenuBuilder()
                .WithCustomId($"minute_select_{eventId}")
                .WithPlaceholder("Select the minute of the event")
                .AddOption("00", "00")
                .AddOption("15", "15")
                .AddOption("30", "30")
                .AddOption("45", "45");

            var builder = new ComponentBuilder().WithSelectMenu(minuteDropdown);

            await RespondAsync("Select the minute of the event:", components: builder.Build(), ephemeral: true);
        }

        [ComponentInteraction("minute_select_*")]
        public async Task HandleMinuteSelected(string customId, string minute)
        {
            int eventId = int.Parse(customId);
            int _minute = int.Parse(minute);

            _requestWorkingCache[eventId].Minute = _minute;

            ScheduleEventRequest scheduleEventRequest = _requestWorkingCache[eventId];

            await RespondToEventRequestAsync(scheduleEventRequest);

            _requestWorkingCache.Remove(scheduleEventRequest.Id);
        }

        private async Task RespondToEventRequestAsync(ScheduleEventRequest scheduleEventRequest)
        {
            await DeferAsync();

            if (scheduleEventRequest.Year is null)
            {
                scheduleEventRequest.Year = DateTime.Now.Year;

                if (scheduleEventRequest.Month < DateTime.Now.Month)
                    ++scheduleEventRequest.Year;
            }

            TimeSpan offset = new(-5, 0, 0);

            DateTimeOffset eventDateTime = new DateTimeOffset(scheduleEventRequest.Year.Value, scheduleEventRequest.Month, scheduleEventRequest.Day, scheduleEventRequest.Hour, scheduleEventRequest.Minute, 0, offset);

            EchelonEvent event_ = new()
            {
                Id = scheduleEventRequest.Id,
                Name = scheduleEventRequest.Name,
                Description = scheduleEventRequest.Description,
                Organizer = Context.User.GlobalName,
                ImageUrl = Context.User.GetAvatarUrl(),
                Footer = _embedFactory.GetRandomFooter(),
                EventDateTime = eventDateTime,
                EventType = scheduleEventRequest.EventType
            };

            IUserMessage message;

            if (scheduleEventRequest.EventType == EventType.Meeting)
                message = await RespondToMeetingEventAsync(event_);
            else
                message = await RespondToGameEventAsync(event_);

            await SaveEventToTableStorage(message.Id, event_);
        }

        private async Task<IUserMessage> RespondToGameEventAsync(EchelonEvent ecEvent)
        {
            MessageComponent components = new ComponentBuilder()
                .WithButton("Sign Up", $"signup_event_{ecEvent.Id}")
                .WithButton("Absence", $"absence_event_{ecEvent.Id}")
                .WithButton("Tentative", $"tentative_event_{ecEvent.Id}")
                .Build();

            Embed embed = _embedFactory.CreateEventEmbed(ecEvent);

            var message = await FollowupAsync(embed: embed, components: components); // Sends the actual message
            return message;
        }

        private async Task<IUserMessage> RespondToMeetingEventAsync(EchelonEvent ecEvent)
        {
            MessageComponent components = new ComponentBuilder()
                .WithButton("Sign Up", $"signupmeeting_event_{ecEvent.Id}")
                .WithButton("Absence", $"absence_event_{ecEvent.Id}")
                .WithButton("Tentative", $"tentative_event_{ecEvent.Id}")
                .Build();

            Embed embed = _embedFactory.CreateEventEmbed(ecEvent);

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
                .Select(a => new AttendeeRecord() 
                { 
                    Id = GetNextAvailableAttendeeRecordId(), 
                    DiscordName = a.DiscordName, 
                    DiscordDisplayName = a.DiscordDisplayName, 
                    EventId = eventId, 
                    Role = a.Role, 
                    Class = a.Class, 
                    Spec = a.Spec 
                })
                .ToList();

            // Rebuild the embed with updated attendees

            EchelonEvent updatedEvent = new()
            {
                Id = int.Parse(eventEntity.RowKey),
                Name = eventEntity.EventName,
                Description = eventEntity.EventDescription,
                Organizer = eventEntity.Organizer,
                ImageUrl = eventEntity.ImageUrl,
                Footer = eventEntity.Footer,
                EventDateTime = eventEntity.EventDateTime,
                EventType = Enum.Parse<EventType>(eventEntity.PartitionKey)
            };

            var embed = _embedFactory.CreateEventEmbed(updatedEvent, attendees);

            // Modify the existing message with the updated embed
            await message.ModifyAsync(msg => msg.Embed = embed);
        }

        private async Task ScheduleEventReminder(EchelonEventEntity ecEvent)
        {
            var scheduledMessageEntity = new ScheduledMessageEntity()
            {
                UserId = Context.User.Id,
                EventId = ecEvent.RowKey,
                SendTime = ecEvent.EventDateTime.AddMinutes(-15),
                Message = CreateReminderMessage(ecEvent)
            };

            await _scheduledMessageTableClient.UpsertEntityAsync(scheduledMessageEntity);
        }

        private string CreateReminderMessage(EchelonEventEntity ecEvent)
        {
            return $"Reminder!\nYou have the event {ecEvent.EventName} at <t:{ecEvent.EventDateTime.ToUnixTimeSeconds()}:F>!";
        }


        // Record response to a meeting or game event.

        [ComponentInteraction("signupmeeting_*")]
        public async Task HandleMeetingSignup(string customId)
        {
            int eventId = int.Parse(customId.Split('_')[1]);

            AttendeeRecord record = new()
            {
                Id = GetNextAvailableAttendeeRecordId(),
                EventId = eventId,
                DiscordDisplayName = Context.User.GlobalName,
                DiscordName = Context.User.Username,
                Role = "Attendee"
            };

            await SaveAttendeeRecordToTableStorage(record);

            await UpdateEventEmbed(eventId);

            var eventEntity = _eventsTableClient.Query<EchelonEventEntity>()
                .FirstOrDefault(e => e.RowKey == eventId.ToString());

            await ScheduleEventReminder(eventEntity);

            await RespondAsync("See you at the meeting!", ephemeral: true);
        }

        [ComponentInteraction("absence_event_*")]
        public async Task HandleAbscence(string customId)
        {
            int eventId = int.Parse(customId);

            AttendeeRecord record = new()
            {
                Id = GetNextAvailableAttendeeRecordId(),
                EventId = eventId,
                DiscordDisplayName = Context.User.GlobalName,
                DiscordName = Context.User.Username,
                Role = "Absent"
            };

            await SaveAttendeeRecordToTableStorage(record);

            await UpdateEventEmbed(eventId);

            await RespondAsync("We'll miss you!", ephemeral: true);

            var scheduledReminders = _scheduledMessageTableClient.Query<ScheduledMessageEntity>(e => e.EventId == customId);

            foreach (ScheduledMessageEntity message in scheduledReminders)
            {
                await _scheduledMessageTableClient.DeleteEntityAsync(message);
            }
        }

        [ComponentInteraction("tentative_event_*")]
        public async Task HandleTentative(string customId)
        {
            int eventId = int.Parse(customId);

            //AttendeeRecord record = new(eventId, Context.User.Username, Context.User.GlobalName, "Tentative", string.Empty, string.Empty);

            AttendeeRecord record = new()
            {
                Id = GetNextAvailableAttendeeRecordId(),
                EventId = eventId,
                DiscordDisplayName = Context.User.GlobalName,
                DiscordName = Context.User.Username,
                Role = "Tentative"
            };

            await SaveAttendeeRecordToTableStorage(record);

            await UpdateEventEmbed(eventId);

            await RespondAsync("We hope to see you!", ephemeral: true);

            var scheduledReminders = _scheduledMessageTableClient.Query<ScheduledMessageEntity>(e => e.EventId == customId);

            foreach (ScheduledMessageEntity message in scheduledReminders)
            {
                await _scheduledMessageTableClient.DeleteEntityAsync(message);
            }
        }

        // Game event signup is a bit more complicated, so here's it's section.
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

            //AttendeeRecord record = new(eventId, Context.User.Username, Context.User.GlobalName, role, selectedClass, selectedSpec);

            AttendeeRecord record = new()
            {
                Id = GetNextAvailableAttendeeRecordId(),
                EventId = eventId,
                DiscordDisplayName = Context.User.GlobalName,
                DiscordName = Context.User.Username,
                Role = role,
                Class = selectedClass,
                Spec = selectedSpec
            };


            await SaveAttendeeRecordToTableStorage(record);

            await UpdateEventEmbed(eventId);

            // Confirm signup
            await RespondAsync($"✅ {user} signed up as a **{selectedSpec.Prettyfy().ToUpper()} {selectedClass.Prettyfy().ToUpper()}** ({role})", ephemeral: true);

            var eventEntity = _eventsTableClient.Query<EchelonEventEntity>()
                .First(e => e.RowKey == eventId.ToString());

            await ScheduleEventReminder(eventEntity);
        }



        private string GetRole(string playerClass, string spec)
        {
            var tanks = new HashSet<string> { "Blood Death Knight", "Guardian Druid", "Brewmaster Monk", "Protection Paladin", "Protection Warrior", "Vengeance Demon Hunter" };
            var healers = new HashSet<string> { "Restoration Druid", "Mistweaver Monk", "Holy Paladin", "Holy Priest", "Discipline Priest", "Restoration Shaman", "Preservation Evoker" };
            var mDps = new HashSet<string>
            {
                "Assassination Rogue",
                "Outlaw Rogue",
                "Subtlety Rogue",
                "Fury Warrior",
                "Arms Warrior",
                "Retribution Paladin",
                "Frost Death Knight",
                "Unholy Death Knight",
                "Enhancement Shaman",
                "Feral Druid",
                "Havoc Demon Hunter",
                "Windwalker Monk",
                "Survival Hunter"
            };

            string fullSpec = $"{spec.Prettyfy()} {playerClass.Prettyfy()}";

            if (tanks.Contains(fullSpec)) return "Tank";
            if (healers.Contains(fullSpec)) return "Healer";
            if (mDps.Contains(fullSpec)) return "Melee DPS";
            return "Ranged DPS";
        }

        private int GetNextAvailableAttendeeRecordId()
        {
            return Random.Shared.Next();

        }
    }
}