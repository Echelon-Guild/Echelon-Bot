namespace EchelonBot.Models
{
    public class EchelonEvent
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Organizer { get; set; }
        public string ImageUrl { get; set; }
        public DateTimeOffset EventDateTime { get; set; }
        public EventType EventType { get; set; }

        public EchelonEvent() { }

        public EchelonEvent(string name, string description, string organizer, string imageUrl, DateTimeOffset eventDateTime, EventType eventType)
        {
            Name = name;
            Description = description;
            Organizer = organizer;
            ImageUrl = imageUrl;
            EventDateTime = eventDateTime;
            EventType = eventType;
        }
    }

    public enum EventType
    {
        Raid,
        Dungeon,
        Meeting
    }
}
