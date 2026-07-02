using AppointmentManagement.Contract;
using BookingAdditionManagement.Contract;
using CrossCutting.DataObjects;
using CrossCutting.Entities;
using CrossCutting.Entities.NotMapped.Smoobu;
using CrossCutting.Language;
using DatabaseManagement.Contract;
using Datastoring.EfCore;
using Microsoft.Extensions.Localization;
using SmoobuManagement.Contract;
using TagManagement.Contract;

namespace AppointmentManagement;

public class AppointmentManager(
    IDatabaseManager<Appointment, PlanerContext> databaseManager,
    ISmoobuManager smoobuManager,
    ITagManager tagManager,
    IBookingAdditionManager bookingAdditionManager,
    IStringLocalizer<Language> localizer)
    : IAppointmentManager
{
    private IEnumerable<Tag> _tags = new List<Tag>();

    public async Task<IEnumerable<Appointment>> GetAllAsync()
    {
        var appointments = await databaseManager.GetAllAsync();
        return appointments;
    }

    public async Task<Appointment?> GetByIdAsync(string id)
    {
        var appointment = await databaseManager.GetByIdAsync(id);
        return appointment;
    }

    public async Task<bool> CreateAsync(Appointment entity)
    {
        var created = await databaseManager.AddAsync(entity);
        return created;
    }

    public async Task<bool> UpdateAsync(Appointment entity, bool ignoreId = false)
    {
        var updated = await databaseManager.UpdateAsync(entity, ignoreId);
        return updated;
    }

    public async Task<bool> DeleteAsync(Appointment entity, bool ignoreId)
    {
        var deleted = await databaseManager.DeleteAsync(entity, ignoreId);
        return deleted;
    }

    public async Task<IEnumerable<CustomCalendarItem>> GetAllWithSmoobuGroupedAsync()
    {
        var rootBooking = await smoobuManager.GetBookingsAsync();
        var appointments = await databaseManager.GetAllAsync();

        var dbAdditions = await bookingAdditionManager.GetAllAsync();
        var bookingAdditions = dbAdditions.ToList();
        var note = string.Empty;

        _tags = await tagManager.GetAllAsync();
        var calendarItems = new List<CustomCalendarItem>();

        foreach (var booking in rootBooking.Bookings!)
        {
            note = bookingAdditions.FirstOrDefault(b => b.BookingId == booking.Id.ToString())?.Note ?? string.Empty;
            calendarItems.Add(new CustomCalendarItem()
            {
                Start = DateTime.ParseExact(booking.Departure, "yyyy-MM-dd", null).Date,
                End = DateTime.ParseExact(booking.Departure, "yyyy-MM-dd", null).Date.AddMinutes(60),
                Text = booking.Apartment?.Name ?? "N/A",
                GuestsCount = booking.Adults ?? 0 + booking.Children ?? 0,
                FutureGuestsCount = GetFuturePeopleCount(rootBooking.Bookings, booking),
                Tag = await GetTagByAccommodationIdAsync(booking.Apartment?.Id.ToString()),
                Color = await GetCellColorAsync(booking.Apartment?.Id.ToString()),
                BookingId = booking.Id.ToString(),
                Note = note
            });
        }

        foreach (var appointment in appointments)
        {
            note = bookingAdditions.FirstOrDefault(b => b.BookingId == appointment.Id)?.Note ?? string.Empty;
            calendarItems.Add(new CustomCalendarItem()
            {
                Start = appointment.Date.Date,
                End = appointment.Date.Date.AddMinutes(60),
                Text = appointment.Tag.Name,
                Tag = _tags.First(t => t.Id == appointment.Tag.Id),
                Color = await GetCellColorAsync(appointment.Tag.Id, false),
                BookingId = appointment.Id,
                Note = note
            });
        }

        var sortedCalendarItems = calendarItems
            .GroupBy(item => item.Start.Date)
            .OrderBy(group => group.Key)
            .SelectMany(group => group
                .OrderByDescending(item => item.Tag.SortIndex)
                .ThenBy(item => item.Start))
            .ToList();

        foreach (var group in sortedCalendarItems
                     .GroupBy(item => item.Start.Date))
        {
            var hourIncrement = 0;
            foreach (var item in group)
            {
                item.Tag.Name = string.IsNullOrEmpty(item.Tag.Name) ? localizer["EmptyTag"] : item.Tag.Name;
                item.Start = item.Start.AddHours(hourIncrement);
                item.End = item.End?.AddHours(hourIncrement);

                hourIncrement++;
            }
        }

        return sortedCalendarItems;
    }

    private async Task<string> GetCellColorAsync(string? accommodationId, bool isBooking = true)
    {
        if (isBooking)
        {
            var tag = _tags.FirstOrDefault(t => t.AccommodationIds.Any(a => a == accommodationId));
            if (tag != null)
            {
                return tag.ColorHexCode;
            }
        }
        else
        {
            var tag = _tags.FirstOrDefault(t => t.Id == accommodationId);
            if (tag != null)
            {
                return tag.ColorHexCode;
            }
        }

        return "grey";
    }

    private async Task<Tag> GetTagByAccommodationIdAsync(string? accommodationId)
    {
        var tag = _tags.FirstOrDefault(t => t.AccommodationIds.Any(a => a == accommodationId));
        return tag ?? new Tag
        {
            Id = "-1",
            Name = string.Empty,
            ColorHexCode = string.Empty,
            IsNonAccommodation = false,
            AccommodationIds = new List<string>(),
            Organization = string.Empty,
            CreatedBy = string.Empty,
            CreatedDate = DateTime.Now,
            UpdatedBy = string.Empty,
            UpdatedDate = DateTime.Now,
            SortIndex = 0
        };
    }

    private int GetFuturePeopleCount(List<Booking> bookings, Booking currentBooking)
    {
        var currentBookings = bookings.Where(b => b.Apartment.Id == currentBooking.Apartment.Id).ToList();

        if (currentBookings.Count() == 1)
            return 0;

        var futureBookingIndex = (currentBookings.ToList().IndexOf(currentBooking) + 1);

        if (futureBookingIndex > (currentBookings.Count - 1))
            return 0;

        var futureBooking = currentBookings[futureBookingIndex];
        var peopleCount = futureBooking.Adults + futureBooking.Children;
        return peopleCount ?? 0;
    }
}