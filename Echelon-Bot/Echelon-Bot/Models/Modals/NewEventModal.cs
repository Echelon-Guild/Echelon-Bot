using Discord.Interactions;
using EchelonBot.Models.TimePicking;
using System.Reflection;

namespace EchelonBot.Models.Modals
{
    public class NewEventModal : IModal
    {
        public string Title { get => "Create a new event"; }

        [InputLabel("Event Name")]
        [ModalTextInput("eventName")]
        public string Name { get; set; }
        [InputLabel("Event Description")]
        [ModalTextInput("eventDescription")]
        public string Description { get; set; }
        [InputLabel("Event Type")]
        [ModalTextInput("eventType")]
        public EventType EventType { get; set; }
        [InputLabel("Year of event")]
        [ModalTextInput("eventYear")]
        public int Year { get; set; }
        [InputLabel("Month of event")]
        [ModalTextInput("eventMonth")]
        public MonthEnum Month { get; set; }
        [InputLabel("Day of event")]
        [ModalTextInput("eventDay")]
        public int Day { get; set; }
        [InputLabel("Hour of event")]
        [ModalTextInput("eventHour")]
        public HourEnum Hour { get; set; }
        [InputLabel("Minute of event")]
        [ModalTextInput("eventMinute")]
        public MinuteEnum Minute { get; set; }

        //private int[] _longMonths = [1, 3, 5, 7, 8, 10, 12];

        //public bool IsLeapYear()
        //{
        //    if (Year == 0) return false;

        //    return (Year % 4 == 0 && Year % 100 != 0) || (Year % 400 == 0);
        //}

        //public bool IsARealDay()
        //{
        //    int numberOfDaysInMonth;

        //    if (_longMonths.Contains((int)Month))
        //    {
        //        numberOfDaysInMonth = 31;
        //    }
        //    else if (Month == MonthEnum.February)
        //    {
        //        if (IsLeapYear())
        //        {
        //            numberOfDaysInMonth = 29;
        //        }
        //        else
        //        {
        //            numberOfDaysInMonth = 28;
        //        }
        //    }
        //    else
        //    {
        //        numberOfDaysInMonth = 30;
        //    }

        //    return Day > 0 && Day <= numberOfDaysInMonth;
        //}
    }
}
