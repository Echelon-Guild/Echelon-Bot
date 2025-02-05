namespace EchelonBot.Models
{
    public class EchelonEvent
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset EventDateTime { get; set; }
        public EventType EventType { get; set; }

        public EchelonEvent() { }

        public EchelonEvent(string name, string description, DateTimeOffset eventDateTime, EventType eventType)
        {
            Name = name;
            Description = description;
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
