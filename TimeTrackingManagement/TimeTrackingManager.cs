using System.Globalization;
using CrossCutting.Entities;
using CrossCutting.Enums;
using DatabaseManagement.Contract;
using Datastoring.EfCore;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using TimeTrackingManagement.Contract;

namespace TimeTrackingManagement;

public class TimeTrackingManager(
    IDatabaseManager<TimeTrackingEntry, PlanerIdentityContext> databaseManager)
    : ITimeTrackingManager
{
    public async Task<IEnumerable<TimeTrackingEntry>> GetAllAsync()
    {
        var dbResults = await databaseManager.GetAllAsync();
        return dbResults;
    }

    public async Task<IEnumerable<TimeTrackingEntry>> GetAllByUserInRange(User user, TimeTrackingView timeTrackingView,
        DateTime date)
    {
        var dbResults = await databaseManager.GetAllAsync(true);
        var filteredResults = dbResults.Where(t =>
            t.Organization == user.Organization.Id && t.User.Id == user.Id);

        filteredResults = await FilterByDateRangeAsync(filteredResults, timeTrackingView, date);

        return filteredResults;
    }

    public async Task<IEnumerable<TimeTrackingEntry>> GetAllByOrgInRange(User user, TimeTrackingView timeTrackingView,
        DateTime date)
    {
        var dbResults = await databaseManager.GetAllAsync(true);
        var filteredResults = dbResults.Where(t =>
            t.Organization == user.Organization.Id);

        filteredResults = await FilterByDateRangeAsync(filteredResults, timeTrackingView, date);

        return filteredResults;
    }

    /// <inheritdoc cref="ITimeTrackingManager.ExportTimeTrackingEntriesAsExcelAsync"/>
    public async Task<byte[]> ExportTimeTrackingEntriesAsExcelAsync(List<TimeTrackingEntry> timeTrackingEntries, string timeEntryDateRange)
    {
        var excelPath = Path.Combine("Resources", "Zeiterfassung.xlsx");

        if (!File.Exists(excelPath))
            throw new FileNotFoundException("Template file does not exist");

        using (var templateStream = new FileStream(excelPath, FileMode.Open, FileAccess.Read))
        {
            using (var memoryStream = new MemoryStream())
            {
                templateStream.CopyTo(memoryStream);

                using (var timeTrackingExcel = SpreadsheetDocument.Open(memoryStream, true))
                {
                    var workbookPart = timeTrackingExcel.WorkbookPart;
                    
                    var groupedEntries = timeTrackingEntries
                        .GroupBy(entry => entry.User.Id)
                        .ToDictionary(group => group.Key, group => group.ToList());
                    
                    AddSummarySheet(workbookPart, groupedEntries, timeEntryDateRange);

                    foreach (var userGroup in groupedEntries)
                    {
                        var user = timeTrackingEntries.First(entry => entry.User.Id == userGroup.Key).User;
                        var userEntries = userGroup.Value;

                        AddSingleEmployeeSheets(user, workbookPart, userEntries);
                    }

                    workbookPart.Workbook.Save();
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream.ToArray();
            }
        }
    }

    private Cell CreateCell(string value, CellValues dataType)
    {
        return new Cell
        {
            CellValue = new CellValue(value),
            DataType = new EnumValue<CellValues>(dataType)
        };
    }

    private void SetCellValue(SheetData sheetData, string cellReference, string value)
    {
        var rowIndex = GetRowIndexFromCellReference(cellReference);
        var row = sheetData.Elements<Row>().FirstOrDefault(r => r.RowIndex == rowIndex);

        if (row == null)
        {
            row = new Row() { RowIndex = rowIndex };
            sheetData.Append(row);
        }

        var cell = row.Elements<Cell>().FirstOrDefault(c => c.CellReference == cellReference);
        if (cell == null)
        {
            cell = new Cell() { CellReference = cellReference };
            row.Append(cell);
        }

        cell.CellValue = new CellValue(value);
        cell.DataType = new EnumValue<CellValues>(CellValues.String);
    }

    private static uint GetRowIndexFromCellReference(string cellReference)
    {
        var numberPart = new string(cellReference.Where(char.IsDigit).ToArray());
        return uint.Parse(numberPart);
    }

    private void AddSummarySheet(WorkbookPart workbookPart, Dictionary<string, List<TimeTrackingEntry>> groupedTimeTrackingEntries, string timeEntryDateRange)
    {
        var sheet = workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault();

        if (sheet == null)
            throw new InvalidOperationException("The template does not contain the summary sheet");

        var worksheetPart = workbookPart.GetPartById(sheet.Id) as WorksheetPart;
        var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
        
        int rowIndex = 2;
        
        SetCellValue(sheetData, "A" + rowIndex, $"Zeitenübersicht - {timeEntryDateRange}");

        foreach (var group in groupedTimeTrackingEntries)
        {
            if(!group.Value.Any())
                continue;

            rowIndex++;
            var user = group.Value.First().User;
            
            SetCellValue(sheetData, "A" + rowIndex, user.FullName);
            SetCellValue(sheetData, "B" + rowIndex, group.Value.Sum(entry => ConvertToDecimalHours(entry.SumHours)).ToString(CultureInfo.InvariantCulture));
            SetCellValue(sheetData, "C" + rowIndex, group.Value.Count.ToString());
        }

        worksheetPart.Worksheet.Save();
    }

    private void AddSingleEmployeeSheets(User user, WorkbookPart workbookPart,
        List<TimeTrackingEntry> userEntries)
    {
        var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        var sheetData = new SheetData();
        worksheetPart.Worksheet = new Worksheet(sheetData);

        var sheetId = (uint)(workbookPart.Workbook.Sheets.Count() + 1);
        var sheet = new Sheet
        {
            Id = workbookPart.GetIdOfPart(worksheetPart),
            SheetId = sheetId,
            Name = user.FullName!.Length > 31 ? user.FullName.Substring(0, 31) : user.FullName
        };

        workbookPart.Workbook.Sheets.Append(sheet);

        var headerRow = new Row();
        headerRow.Append(
            CreateCell("Name", CellValues.String),
            CreateCell("Start", CellValues.String),
            CreateCell("Ende", CellValues.String),
            CreateCell("Stunden", CellValues.String)
        );
        sheetData.Append(headerRow);

        foreach (var entry in userEntries)
        {
            var newRow = new Row();
            newRow.Append(
                CreateCell(entry.User.FullName, CellValues.String),
                CreateCell(entry.Start.ToString("dd.MM.yyyy HH:mm"), CellValues.String),
                CreateCell(entry.End?.ToString("dd.MM.yyyy HH:mm") ?? "Noch aktiv", CellValues.String),
                CreateCell(ConvertToDecimalHours(entry.SumHours).ToString(CultureInfo.InvariantCulture), CellValues.String)
            );
            sheetData.Append(newRow);
        }

        var summaryRow = new Row();
        summaryRow.Append(
            CreateCell("Gesamt", CellValues.String),
            CreateCell(string.Empty, CellValues.String),
            CreateCell(string.Empty, CellValues.String),
            CreateCell(
                userEntries.Sum(entry => ConvertToDecimalHours(entry.SumHours))
                    .ToString(CultureInfo.InvariantCulture),
                CellValues.String
            )
        );

        sheetData.Append(summaryRow);

        worksheetPart.Worksheet.Save();
    }

    decimal ConvertToDecimalHours(string timeString)
    {
        if (string.IsNullOrWhiteSpace(timeString))
            return 0m;

        var timeParts = timeString.Split(':');
        if (timeParts.Length != 2)
            throw new FormatException($"Invalid time format: {timeString}");

        if (!int.TryParse(timeParts[0], out int hours) || !int.TryParse(timeParts[1], out int minutes))
            throw new FormatException($"Invalid time format: {timeString}");

        return hours + (minutes / 60m);
    }

    private async Task<IEnumerable<TimeTrackingEntry>> FilterByDateRangeAsync(
        IEnumerable<TimeTrackingEntry> timeTrackingEntries,
        TimeTrackingView timeTrackingView,
        DateTime date)
    {
        DateTime startDate = default, endDate = default;
        switch (timeTrackingView)
        {
            case TimeTrackingView.Weekly:
                var deltaToMonday = DayOfWeek.Monday - date.DayOfWeek;
                startDate = date.AddDays(deltaToMonday);
                endDate = startDate.AddDays(6);
                break;
            case TimeTrackingView.Monthly:
                startDate = new DateTime(date.Year, date.Month, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);
                break;
        }

        timeTrackingEntries =
            timeTrackingEntries.Where(t => t.Start.Date >= startDate.Date && t.Start.Date <= endDate.Date);
        return timeTrackingEntries;
    }

    public async Task<TimeTrackingEntry?> GetByIdAsync(string id)
    {
        var dbResult = await databaseManager.GetByIdAsync(id);
        return dbResult;
    }

    public async Task<bool> CreateAsync(TimeTrackingEntry entity)
    {
        var created = await databaseManager.AddAsync(entity);
        return created;
    }

    public async Task<bool> UpdateAsync(TimeTrackingEntry entity, bool ignoreId = false)
    {
        var updated = await databaseManager.UpdateAsync(entity, ignoreId);
        return updated;
    }

    public async Task<bool> DeleteAsync(TimeTrackingEntry entity, bool ignoreId)
    {
        var deleted = await databaseManager.DeleteAsync(entity, ignoreId);
        return deleted;
    }
}