using Discord;
using EchelonBot.Models;
using EchelonBot.Models.Entities;
using EchelonBot.Models.WoW;
using System.Text;

namespace EchelonBot.Services
{
    public class EmbedFactory
    {
        private EmoteFinder _emoteFinder;

        public EmbedFactory(EmoteFinder emoteFinder)
        {
            _emoteFinder = emoteFinder;
        }

        public Embed CreateEventEmbed(EchelonEvent ecEvent, IEnumerable<AttendeeRecord> attendees = null)
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

            string timestamp = $"<t:{ecEvent.EventDateTime.ToUnixTimeSeconds()}:F>";

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle(ecEvent.Name)
                .WithDescription(ecEvent.Description)
                .WithColor(color)
                .AddField("Scheduled Time", timestamp)
                .AddField("Event Type", ecEvent.EventType.ToString(), true)
                .AddField("Organizer", ecEvent.Organizer, true)
                .WithThumbnailUrl(ecEvent.ImageUrl)
                .WithFooter($"Powered by: {ecEvent.Footer}");

            if (attendees != null)
            {
                if (ecEvent.EventType == EventType.Meeting)
                {
                    IEnumerable<AttendeeRecord> attending = attendees.Where(e => e.Role.ToLower() == "attendee");

                    if (attending.Any())
                        embed.AddField($"✅ Attendees ({attending.Count()})", GetMeetingAttendeeString(attending));
                }
                else
                {
                    IEnumerable<AttendeeRecord> tanks = attendees.Where(e => e.Role.ToLower() == "tank");
                    IEnumerable<AttendeeRecord> healers = attendees.Where(e => e.Role.ToLower() == "healer");
                    IEnumerable<AttendeeRecord> mdps = attendees.Where(e => e.Role.ToLower() == "melee dps");
                    IEnumerable<AttendeeRecord> rdps = attendees.Where(e => e.Role.ToLower() == "ranged dps");


                    if (tanks.Any())
                        embed.AddField($"🛡️ Tanks ({tanks.Count()})", GetGameEventAttendeeString(tanks));

                    if (healers.Any())
                        embed.AddField($"❤️ Healers ({healers.Count()})", GetGameEventAttendeeString(healers));

                    if (mdps.Any())
                        embed.AddField($"🗡️ Melee DPS ({mdps.Count()})", GetGameEventAttendeeString(mdps));

                    if (rdps.Any())
                        embed.AddField($"🏹 Ranged DPS ({rdps.Count()})", GetGameEventAttendeeString(rdps));
                }

                IEnumerable<AttendeeRecord> absent = attendees.Where(e => e.Role.ToLower() == "absent");

                if (absent.Any())
                    embed.AddField($"❌ Absent ({absent.Count()})", GetMeetingAttendeeString(absent));

                IEnumerable<AttendeeRecord> tentative = attendees.Where(e => e.Role.ToLower() == "tentative");

                if (tentative.Any())
                    embed.AddField($"\U0001f9c7 Tentative ({tentative.Count()})", GetMeetingAttendeeString(tentative));
            }

            return embed.Build();
        }

        private string GetMeetingAttendeeString(IEnumerable<AttendeeRecord> attendees)
        {
            if (!attendees.Any())
                return string.Empty;

            StringBuilder sb = new();

            foreach (AttendeeRecord attendee in attendees)
            {
                sb.AppendLine($"{attendee.DiscordDisplayName}");
            }

            return sb.ToString() ?? string.Empty;
        }

        private string GetGameEventAttendeeString(IEnumerable<AttendeeRecord> attendees)
        {
            if (!attendees.Any())
                return string.Empty;

            StringBuilder sb = new();
            foreach (AttendeeRecord attendee in attendees)
            {
                sb.AppendLine($"{GetAttendeeEmote(attendee)} {attendee.DiscordDisplayName}");
            }

            return sb.ToString() ?? string.Empty;
        }

        private string GetAttendeeEmote(AttendeeRecord attendee)
        {
            string role = attendee.Role.ToLower();

            if (role == "absent")
                return "❌";

            if (role == "tentative")
                return "🧇";

            if (role == "attendee")
                return "✅";

            //TODO: Identify custom emotes based on class and spec for WoW events. Non-wow events are handled above.

            string attendeeEmoteCode = _emoteFinder.GetEmote(attendee.Class, attendee.Spec);

            if (!string.IsNullOrWhiteSpace(attendeeEmoteCode))
            {
                return $"<:{attendee.Spec.ToLower()}:{attendeeEmoteCode}>";
            }

            return "<:rocket:1234567890>";
        }

        public string GetRandomFooter()
        {
            string[] possibleFooters =
            [
                "Frenzied Regeneration",
                "EggBoi",
                "Having a Good Time",
                "Zug Zug",
                "Brain Cell",
                "Stay Close to the Nipple",
                "On me! On me!",
                "MY BODY!",
                "Witawy",
                "Pet Pulling...",
            ];

            int footerToReturn = Random.Shared.Next(0, possibleFooters.Length);

            return possibleFooters[footerToReturn];

        }


        public Embed CreateInstanceEmbed(WoWInstanceInfoEntity instanceInfo)
        {
            Color color = instanceInfo.Legacy ? Color.Red : Color.Green;

            string description = instanceInfo.Legacy ? $"Legacy {instanceInfo.PartitionKey}" : instanceInfo.PartitionKey;

            Console.WriteLine(instanceInfo.ImageUrl);

            return new EmbedBuilder()
                .WithTitle(instanceInfo.Name)
                .WithDescription(description)
                .WithColor(color)
                .AddField("Database ID", instanceInfo.RowKey)
                .WithThumbnailUrl(instanceInfo.ImageUrl)
                .Build();
        }

        public Embed CreateInstanceEmbed(IEnumerable<WoWInstanceInfoEntity> instanceInfos)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Stored Instances")
                .WithDescription("Here are the instances currently stored in the database.")
                .WithColor(Color.Green);

            var raids = instanceInfos.Where(e => e.PartitionKey == InstanceType.Raid.ToString());
            var dungeons = instanceInfos.Where(e => e.PartitionKey == InstanceType.Dungeon.ToString());

            if (raids.Any())
            {
                embedBuilder.AddField("__Raids__", GetStoredInstanceString(raids));
            }

            if (dungeons.Any())
            {
                embedBuilder.AddField("__Dungeons__", GetStoredInstanceString(dungeons));
            }

            return embedBuilder.Build();
                
        }

        private string GetStoredInstanceString(IEnumerable<WoWInstanceInfoEntity> instances)
        {
            StringBuilder sb = new();

            foreach (WoWInstanceInfoEntity instance in instances)
            {
                sb.AppendLine($"{instance.Name}");
                sb.AppendLine($"ID: {instance.RowKey}");
            }

            return sb.ToString();
        }
    }
}
