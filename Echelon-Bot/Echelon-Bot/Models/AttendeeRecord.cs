using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchelonBot.Models
{
    public class AttendeeRecord
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        [StringLength(32)]
        public string DiscordName { get; set; }
        public string DiscordDisplayName { get; set; }
        public string Role { get; set; }
        [StringLength(16)]
        public string? Class { get; set; }
        [StringLength(16)]
        public string? Spec { get; set; }

        public AttendeeRecord() { }

        public AttendeeRecord(int eventId, string discordName, string discordDisplayName, string role, string _class, string spec)
        {
            EventId = eventId;
            DiscordName = discordName;
            DiscordDisplayName = discordDisplayName;
            Role = role;
            Class = _class;
            Spec = spec;
        }

        public AttendeeRecord(int eventId, string discordName, string discordDisplayName, string role)
        {
            EventId = eventId;
            DiscordName = discordName;
            DiscordDisplayName = discordDisplayName;
            Role = role;
        }


    }
}
