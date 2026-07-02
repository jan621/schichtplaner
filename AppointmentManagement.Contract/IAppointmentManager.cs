using CrossCutting.DataObjects;
using CrossCutting.Entities;
using CrossCutting.Managers;

namespace AppointmentManagement.Contract;

public interface IAppointmentManager : IEntityManager<Appointment>
{
    Task<IEnumerable<CustomCalendarItem>> GetAllWithSmoobuGroupedAsync();
}