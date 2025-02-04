namespace EchelonBot.Models
{
    public class ScheduleEventRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public EventType EventType { get; set; }
        public int Month { get; set; }
        public int Week { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public string AmOrPm { get; set; } = "AM";

        public DateTimeOffset GetEventDateEasternTime()
        {
            TimeSpan offset = new(-5, 0, 0);

            int year = DateTimeOffset.Now.Year;

            if (Month < DateTimeOffset.Now.Month)
            {
                ++year;
            }

            int _hour = Hour;

            if (AmOrPm.ToLower() == "pm")
                _hour += 12;

            return new DateTimeOffset(year, Month, Day, _hour, Minute, 0, offset);
        }
    }
}
