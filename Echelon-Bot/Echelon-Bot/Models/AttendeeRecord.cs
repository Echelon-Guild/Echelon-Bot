using System.ComponentModel.DataAnnotations;

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
    }
}
