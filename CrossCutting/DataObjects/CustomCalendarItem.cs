using CrossCutting.Entities;
using Heron.MudCalendar;

namespace CrossCutting.DataObjects;

public class CustomCalendarItem : CalendarItem
{
    public string Color { get; set; } = "black";

    public int GuestsCount { get; set; } = -1;

    public int FutureGuestsCount { get; set; } = -1;

    public required Tag Tag { get; set; }
    
    public required string BookingId { get; set; }

    public string Note { get; set; } = string.Empty;
}