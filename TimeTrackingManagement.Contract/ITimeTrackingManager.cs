using CrossCutting.Entities;
using CrossCutting.Enums;
using CrossCutting.Managers;

namespace TimeTrackingManagement.Contract;

public interface ITimeTrackingManager: IEntityManager<TimeTrackingEntry>
{
    Task<IEnumerable<TimeTrackingEntry>> GetAllByUserInRange(User user, TimeTrackingView timeTrackingView, DateTime date);
    
    Task<IEnumerable<TimeTrackingEntry>> GetAllByOrgInRange(User user, TimeTrackingView timeTrackingView, DateTime date);

    /// <summary>
    /// Exports a list of <see cref="TimeTrackingEntry"/> as an Excel file
    /// </summary>
    /// <param name="timeTrackingEntries">List of entries</param>
    /// <param name="timeEntryDateRange">Date range of time entry</param>
    /// <returns>A task containing byte[]</returns>
    Task<byte[]> ExportTimeTrackingEntriesAsExcelAsync(List<TimeTrackingEntry> timeTrackingEntries, string timeEntryDateRange);
}