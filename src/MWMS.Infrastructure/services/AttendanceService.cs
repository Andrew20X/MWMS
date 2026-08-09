using MWMS.Application.DTOs;
using MWMS.Application.DTOs.Attendance;
using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;
using MWMS.Domain.Enums;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;
namespace MWMS.Infrastructure.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IOvertimeRepository _overtimeRepository;
    private readonly MWMS.Application.Interfaces.IGenericRepository<RawAttendanceLog> _rawLogRepository;
    private readonly ISalaryDeductionRepository _deductionRepository;
    private readonly MWMS.Application.Interfaces.IGenericRepository<LeaveRequest> _leaveRequestRepository;

    public AttendanceService(
        IAttendanceRepository attendanceRepository, 
        IEmployeeRepository employeeRepository,
        IOvertimeRepository overtimeRepository,
        MWMS.Application.Interfaces.IGenericRepository<RawAttendanceLog> rawLogRepository,
        ISalaryDeductionRepository deductionRepository,
        MWMS.Application.Interfaces.IGenericRepository<LeaveRequest> leaveRequestRepository)
    {
        _attendanceRepository = attendanceRepository;
        _employeeRepository = employeeRepository;
        _overtimeRepository = overtimeRepository;
        _rawLogRepository = rawLogRepository;
        _deductionRepository = deductionRepository;
        _leaveRequestRepository = leaveRequestRepository;
    }

    public async Task DeleteMyAttendanceAsync(int employeeId)
    {
        var myDeductions = await _deductionRepository.GetByEmployeeAsync(employeeId);
        foreach (var d in myDeductions)
        {
            _deductionRepository.Delete(d);
        }
        await _deductionRepository.SaveChangesAsync();

        var allLeaves = (await _leaveRequestRepository.GetAllAsync())
            .Where(l => l.EmployeeId == employeeId && l.LinkedAttendanceId != null)
            .ToList();
        foreach (var l in allLeaves)
        {
            l.LinkedAttendanceId = null;
            _leaveRequestRepository.Update(l);
        }
        await _leaveRequestRepository.SaveChangesAsync();

        var attendances = await _attendanceRepository.GetByEmployeeAsync(employeeId);
        foreach (var attendance in attendances)
        {
            _attendanceRepository.Delete(attendance);
        }
        await _attendanceRepository.SaveChangesAsync();
    }

    public async Task DeleteAllRawAttendanceAsync()
    {
        // First delete dependent salary deductions to avoid FK constraint errors
        var allDeductions = (await _deductionRepository.GetAllAsync()).ToList();
        foreach (var d in allDeductions)
        {
            _deductionRepository.Delete(d);
        }
        await _deductionRepository.SaveChangesAsync();

        // Unlink LeaveRequests to avoid FK constraint errors
        var allLeaves = (await _leaveRequestRepository.GetAllAsync()).Where(l => l.LinkedAttendanceId != null).ToList();
        foreach (var l in allLeaves)
        {
            l.LinkedAttendanceId = null;
            _leaveRequestRepository.Update(l);
        }
        await _leaveRequestRepository.SaveChangesAsync();

        // Delete final processed attendances
        var allAttendances = (await _attendanceRepository.GetAttendancesByDateRangeAsync(DateOnly.MinValue, DateOnly.MaxValue)).ToList();
        
        foreach(var a in allAttendances) 
        {
            _attendanceRepository.Delete(a);
        }
        await _attendanceRepository.SaveChangesAsync();

        // Also delete raw machine logs, so they can be re-fetched
        var allRawLogs = (await _rawLogRepository.GetAllAsync()).ToList();
        foreach (var r in allRawLogs)
        {
            _rawLogRepository.Delete(r);
        }
        await _rawLogRepository.SaveChangesAsync();
    }

    public async Task<CheckInResponseDto> CheckInAsync(int employeeId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);

        if (employee == null)
        {
            throw new InvalidOperationException("Employee not found.");
        }

        var today = DateOnly.FromDateTime(DateTime.Now);

        var existingAttendance =
            await _attendanceRepository.GetByEmployeeAndDateAsync(employee.Id, today);

        if (existingAttendance != null)
        {
            throw new InvalidOperationException("Employee has already checked in today.");
        }

        var now = TimeOnly.FromDateTime(DateTime.Now);
        var shiftStart = employee.Shift.StartTime;
        var lateMinutes = 0;
        var status = AttendanceStatus.Present;

        if (now > shiftStart.AddMinutes(employee.Shift.GraceMinutes))
        {
            lateMinutes = (int)(now.ToTimeSpan() - shiftStart.ToTimeSpan()).TotalMinutes;
            status = AttendanceStatus.Late;
        }

        var attendance = new Attendance
        {
            EmployeeId = employee.Id,
            Date = today,
            CheckIn = now,
            Status = status,
            LateMinutes = lateMinutes
        };

        await _attendanceRepository.AddAsync(attendance);
        await _attendanceRepository.SaveChangesAsync();

        return new CheckInResponseDto
        {
            Success = true,
            Message = "Checked in successfully.",
            CheckInTime = attendance.CheckIn!.Value,
            IsLate = attendance.Status == AttendanceStatus.Late,
            LateMinutes = attendance.LateMinutes
        };
    }

    public async Task<AttendanceResponseDto?> CheckOutAsync(int employeeId)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        var attendance = await _attendanceRepository
            .GetByEmployeeAndDateAsync(employeeId, today);

        if (attendance == null)
        {
            throw new InvalidOperationException("Employee has not checked in today.");
        }

        if (attendance.CheckOut != null)
        {
            throw new InvalidOperationException("Employee has already checked out.");
        }

        var employee = await _employeeRepository.GetByIdAsync(employeeId);

        if (employee == null)
        {
            throw new InvalidOperationException("Employee not found.");
        }

        var now = TimeOnly.FromDateTime(DateTime.Now);

        attendance.CheckOut = now;

        var worked = now.ToTimeSpan() - attendance.CheckIn!.Value.ToTimeSpan();
        attendance.WorkedHours = worked.TotalHours;

        var shiftEnd = employee.Shift.EndTime;

        if (now < shiftEnd)
        {
            attendance.EarlyLeaveMinutes =
                (int)(shiftEnd.ToTimeSpan() - now.ToTimeSpan()).TotalMinutes;
        }
        else
        {
            attendance.OvertimeMinutes =
                (int)(now.ToTimeSpan() - shiftEnd.ToTimeSpan()).TotalMinutes;
        }

        _attendanceRepository.Update(attendance);
        await _attendanceRepository.SaveChangesAsync();

        return new AttendanceResponseDto
        {
            EmployeeId = attendance.EmployeeId,
            EmployeeCode = employee.EmployeeCode,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            Date = attendance.Date,
            CheckIn = attendance.CheckIn,
            CheckOut = attendance.CheckOut,
            Status = attendance.Status.ToString(),
            WorkedHours = attendance.WorkedHours,
            LateMinutes = attendance.LateMinutes,
            EarlyLeaveMinutes = attendance.EarlyLeaveMinutes,
            OvertimeMinutes = attendance.OvertimeMinutes
        };
    }

    public async Task<IEnumerable<AttendanceResponseDto>> GetTodayAttendanceAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        var attendances = await _attendanceRepository.GetTodayAttendanceAsync(today);

        return attendances.Select(a => new AttendanceResponseDto
        {
            EmployeeId = a.EmployeeId,
            EmployeeCode = a.Employee.EmployeeCode,
            EmployeeName = $"{a.Employee.FirstName} {a.Employee.LastName}",
            Date = a.Date,
            CheckIn = a.CheckIn,
            CheckOut = a.CheckOut,
            Status = a.Status.ToString(),
            WorkedHours = a.WorkedHours,
            LateMinutes = a.LateMinutes,
            EarlyLeaveMinutes = a.EarlyLeaveMinutes,
            OvertimeMinutes = a.OvertimeMinutes
        });
    }

    public async Task<IEnumerable<AttendanceResponseDto>> GetRecentAttendanceAsync(int limit = 50)
    {
        var attendances = await _attendanceRepository.GetRecentAttendanceAsync(limit);

        return attendances.Select(a => new AttendanceResponseDto
        {
            EmployeeId = a.EmployeeId,
            EmployeeCode = a.Employee.EmployeeCode,
            EmployeeName = $"{a.Employee.FirstName} {a.Employee.LastName}",
            Date = a.Date,
            CheckIn = a.CheckIn,
            CheckOut = a.CheckOut,
            Status = a.Status.ToString(),
            WorkedHours = a.WorkedHours,
            LateMinutes = a.LateMinutes,
            EarlyLeaveMinutes = a.EarlyLeaveMinutes,
            OvertimeMinutes = a.OvertimeMinutes
        });
    }

    public async Task<IEnumerable<AttendanceResponseDto>> GetEmployeeAttendanceAsync(int employeeId)
    {
        var attendances = await _attendanceRepository.GetByEmployeeAsync(employeeId);
        var overtimes = await _overtimeRepository.GetByEmployeeAsync(employeeId);
        var periodOvertimes = overtimes.Where(o => o.Status == "Approved").GroupBy(o => o.Date).ToDictionary(g => g.Key, g => g.ToList());

        var result = attendances.Select(a => new AttendanceResponseDto
        {
            EmployeeId = a.EmployeeId,
            EmployeeCode = a.Employee.EmployeeCode,
            EmployeeName = $"{a.Employee.FirstName} {a.Employee.LastName}",
            Date = a.Date,
            CheckIn = a.CheckIn,
            CheckOut = a.CheckOut,
            Status = a.Status.ToString(),
            WorkedHours = a.WorkedHours,
            LateMinutes = a.LateMinutes,
            EarlyLeaveMinutes = a.EarlyLeaveMinutes,
            OvertimeMinutes = a.OvertimeMinutes,
            OvertimeType = periodOvertimes.TryGetValue(a.Date, out var ots) && ots.Any() ? ots.First().Type : null
        }).ToList();

        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee != null)
        {
            foreach (var otGroup in periodOvertimes)
            {
                if (!attendances.Any(a => a.Date == otGroup.Key))
                {
                    var ot = otGroup.Value.First();
                    var otSpan = ot.EndTime.ToTimeSpan() - ot.StartTime.ToTimeSpan();
                    if (otSpan.TotalMinutes < 0) otSpan = otSpan.Add(TimeSpan.FromHours(24));
                    
                    result.Add(new AttendanceResponseDto
                    {
                        EmployeeId = employee.Id,
                        EmployeeCode = employee.EmployeeCode,
                        EmployeeName = $"{employee.FirstName} {employee.LastName}",
                        Date = ot.Date,
                        CheckIn = ot.StartTime,
                        CheckOut = ot.EndTime,
                        Status = "Overtime",
                        WorkedHours = otSpan.TotalHours,
                        OvertimeMinutes = (int)otSpan.TotalMinutes,
                        OvertimeType = ot.Type
                    });
                }
            }
        }

        return result.OrderByDescending(r => r.Date);
    }

    public async Task<int> ImportTimesheetAsync(Stream excelStream, int? expectedEmployeeId = null)
    {
        string? expectedEmployeeCode = null;
        if (expectedEmployeeId.HasValue)
        {
            var expectedEmployee = await _employeeRepository.GetByIdAsync(expectedEmployeeId.Value);
            if (expectedEmployee != null)
            {
                expectedEmployeeCode = expectedEmployee.EmployeeCode;
            }
        }

        using var workbook = new XLWorkbook(excelStream);
        var worksheet = workbook.Worksheet(1);
        var rows = worksheet.RangeUsed()?.RowsUsed();
        if (rows == null) return 0;

        int importedCount = 0;
        var rawLogs = new List<(string EmpCode, DateTime PunchTime)>();

        foreach (var row in rows)
        {
            var empCode = row.Cell(2).Value.ToString().Trim(); // Column B: ID
            
            // Skip obvious headers or empty rows
            if (string.IsNullOrWhiteSpace(empCode) || 
                empCode.Equals("ID", StringComparison.OrdinalIgnoreCase) || 
                empCode.Equals("No.", StringComparison.OrdinalIgnoreCase) ||
                empCode.Equals("Employee ID", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            
            if (expectedEmployeeId.HasValue)
            {
                if (empCode != expectedEmployeeId.Value.ToString() &&
                    !string.Equals(empCode, expectedEmployeeCode, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals($"EMP-SYNC-{empCode}", expectedEmployeeCode, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(empCode, $"EMP-SYNC-{expectedEmployeeCode}", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals($"EMP-{empCode}", expectedEmployeeCode, StringComparison.OrdinalIgnoreCase))
                {
                    var displayId = expectedEmployeeCode?.Replace("EMP-SYNC-", "", StringComparison.OrdinalIgnoreCase)?.Replace("EMP-", "", StringComparison.OrdinalIgnoreCase) ?? expectedEmployeeId.Value.ToString();
                    throw new InvalidOperationException($"ID mismatch: File contains ID '{empCode}' that doesn't match your ID '{displayId}'.");
                }
            }

            DateTime punchTime;
            var cellDate = row.Cell(3); // Column C: Date and Time
            
            if (cellDate.DataType == XLDataType.DateTime)
            {
                punchTime = cellDate.GetDateTime();
            }
            else if (cellDate.DataType == XLDataType.Number)
            {
                punchTime = DateTime.FromOADate(cellDate.GetDouble());
            }
            else
            {
                var dateStr = cellDate.GetString().Trim();
                if (!DateTime.TryParse(dateStr, out punchTime))
                {
                    // Try exact formats as a fallback
                    string[] formats = { "M/d/yyyy H:mm", "M/d/yyyy H:mm:ss", "MM/dd/yyyy HH:mm:ss", "dd/MM/yyyy HH:mm:ss", "yyyy-MM-dd HH:mm:ss" };
                    if (!DateTime.TryParseExact(dateStr, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out punchTime))
                    {
                        continue; // If completely unparseable, skip this row
                    }
                }
            }

            rawLogs.Add((empCode, punchTime));
        }

        var groupedLogs = rawLogs
            .GroupBy(l => new { l.EmpCode, Date = DateOnly.FromDateTime(l.PunchTime) })
            .ToList();

        foreach (var group in groupedLogs)
        {
            var empCode = group.Key.EmpCode;
            var date = group.Key.Date;
            var punches = group.OrderBy(p => p.PunchTime).ToList();

            if (!punches.Any()) continue;

            var firstPunch = TimeOnly.FromDateTime(punches.First().PunchTime);
            var lastPunch = TimeOnly.FromDateTime(punches.Last().PunchTime);

            var employee = await _employeeRepository.GetByEmployeeCodeAsync(empCode) 
                        ?? await _employeeRepository.GetByEmployeeCodeAsync($"EMP-SYNC-{empCode}");
            if (employee == null) continue;

            var shift = employee.Shift;

            var attendance = await _attendanceRepository.GetByEmployeeAndDateAsync(employee.Id, date);
            bool isNew = false;
            if (attendance == null)
            {
                attendance = new Attendance
                {
                    EmployeeId = employee.Id,
                    Date = date
                };
                isNew = true;
            }

            attendance.CheckIn = firstPunch;

            if (punches.Count > 1)
            {
                attendance.CheckOut = lastPunch;
            }
            else
            {
                attendance.CheckOut = null;
            }

            // Calculate metrics
            if (shift != null && attendance.CheckIn > shift.StartTime.AddMinutes(shift.GraceMinutes))
            {
                attendance.Status = AttendanceStatus.Late;
                attendance.LateMinutes = (int)(attendance.CheckIn.Value.ToTimeSpan() - shift.StartTime.ToTimeSpan()).TotalMinutes;
            }
            else
            {
                attendance.Status = AttendanceStatus.Present;
                attendance.LateMinutes = 0;
            }

            if (attendance.CheckOut.HasValue)
            {
                attendance.WorkedHours = (attendance.CheckOut.Value.ToTimeSpan() - attendance.CheckIn.Value.ToTimeSpan()).TotalHours;

                if (shift != null)
                {
                    if (attendance.CheckOut.Value < shift.EndTime)
                    {
                        attendance.EarlyLeaveMinutes = (int)(shift.EndTime.ToTimeSpan() - attendance.CheckOut.Value.ToTimeSpan()).TotalMinutes;
                        attendance.OvertimeMinutes = 0;
                    }
                    else if (attendance.CheckOut.Value > shift.EndTime)
                    {
                        attendance.OvertimeMinutes = (int)(attendance.CheckOut.Value.ToTimeSpan() - shift.EndTime.ToTimeSpan()).TotalMinutes;
                        attendance.EarlyLeaveMinutes = 0;
                    }
                }
            }

            if (isNew)
                await _attendanceRepository.AddAsync(attendance);
            else
                _attendanceRepository.Update(attendance);
                
            importedCount++;
        }

        await _attendanceRepository.SaveChangesAsync();
        return importedCount;
    }

    public async Task<byte[]> ExportEmployeeTimesheetAsync(int employeeId, DateOnly startDate, DateOnly endDate, string templatePath)
    {
        using var workbook = new XLWorkbook(templatePath);
        var worksheet = workbook.Worksheet(1);

        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee != null)
        {
            worksheet.Cell(6, 3).Value = $"{employee.FirstName} {employee.LastName}"; // Col C, Row 6: NAME
            worksheet.Cell(6, 7).Value = employee.Position?.Name ?? ""; // Col G, Row 6: POSITION
            worksheet.Cell(6, 10).Value = employee.Department?.Name ?? ""; // Col J, Row 6: DEP.
        }

        worksheet.Cell(7, 3).Value = startDate.Month; // Col C, Row 7: MONTH
        worksheet.Cell(7, 7).Value = startDate.Year; // Col G, Row 7: YEAR
        worksheet.Cell(8, 3).Value = $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}"; // Col C, Row 8: PERIOD

        var attendances = await _attendanceRepository.GetByEmployeeAsync(employeeId);
        var periodAttendances = attendances
            .Where(a => a.Date >= startDate && a.Date <= endDate)
            .GroupBy(a => a.Date)
            .ToDictionary(g => g.Key, g => g.First());

        var overtimeRequests = await _overtimeRepository.GetByEmployeeAsync(employeeId);
        var periodOvertimes = overtimeRequests
            .Where(o => o.Date >= startDate && o.Date <= endDate && o.Status == "Approved")
            .GroupBy(o => o.Date)
            .ToDictionary(g => g.Key, g => g.ToList());

        var deductions = await _deductionRepository.GetByEmployeeAsync(employeeId);
        var periodDeductions = deductions
            .Where(d => d.RelatedAttendance != null && d.RelatedAttendance.Date >= startDate && d.RelatedAttendance.Date <= endDate && (d.Status == PayrollStatus.Waived || d.Status == PayrollStatus.Rejected))
            .GroupBy(d => d.RelatedAttendance.Date)
            .ToDictionary(g => g.Key, g => g.ToList());

        int startRow = 13;
        
        // 1. Clear all 31 days (rows 13 to 43) contents so no old template data is left
        for (int i = 0; i < 31; i++)
        {
            var r = startRow + i;
            // Do NOT clear columns 2 (Day) and 3 (Date) to preserve template formulas
            worksheet.Cell(r, 4).Clear(XLClearOptions.Contents); // Duty Code
            worksheet.Cell(r, 5).Clear(XLClearOptions.Contents); // Time In
            worksheet.Cell(r, 6).Clear(XLClearOptions.Contents); // Time Out
            worksheet.Cell(r, 12).Clear(XLClearOptions.Contents); // Description
        }

            int daysDiff = (endDate.DayNumber - startDate.DayNumber) + 1;
        if (daysDiff > 31) daysDiff = 31; // Prevent going beyond standard template

        for (int i = 0; i < daysDiff; i++)
        {
            var date = startDate.AddDays(i);
            var row = startRow + i; // Write sequentially starting from row 13
            
            // Overwrite columns 2 (Day Name) and 3 (Date) with actual values
            worksheet.Cell(row, 2).Value = date.ToString("dddd"); // e.g. Monday
            worksheet.Cell(row, 3).Value = date.ToDateTime(TimeOnly.MinValue); // Insert as DateTime instead of string

            if (periodAttendances.TryGetValue(date, out var attendance))
            {
                string dutyCode = "OD";
                if (periodOvertimes.TryGetValue(date, out var ots) && ots.Any())
                {
                    dutyCode = ots.First().Type;
                }
                worksheet.Cell(row, 4).Value = dutyCode; // Duty Code
                
                if (attendance.CheckIn.HasValue)
                    worksheet.Cell(row, 5).Value = attendance.CheckIn.Value.ToTimeSpan(); 
                
                if (attendance.CheckOut.HasValue)
                    worksheet.Cell(row, 6).Value = attendance.CheckOut.Value.ToTimeSpan();
                    
                string desc = attendance.Status.ToString();

                if (attendance.Status == MWMS.Domain.Enums.AttendanceStatus.Present)
                {
                    worksheet.Cell(row, 7).Value = TimeSpan.Zero; // Late
                    worksheet.Cell(row, 8).Value = TimeSpan.Zero; // Early Leave
                }

                if (periodOvertimes.TryGetValue(date, out var overtimes))
                {
                    var otDesc = string.Join(", ", overtimes.Select(o => $"{o.Type} from {o.StartTime} to {o.EndTime}"));
                    desc = desc + " | " + otDesc;
                }
                
                var deducDescList = new List<string>();
                if (periodDeductions.TryGetValue(date, out var deducs))
                {
                    foreach (var d in deducs)
                    {
                        if (d.Status == PayrollStatus.Rejected)
                        {
                            var msg = "Deduction Approved";
                            if (!string.IsNullOrWhiteSpace(d.RejectionReason)) msg += $": {d.RejectionReason}";
                            deducDescList.Add(msg);
                        }
                        else if (d.Status == PayrollStatus.Waived)
                        {
                            var msg = "Deduction Rejected/Waived";
                            if (!string.IsNullOrWhiteSpace(d.RejectionReason)) msg += $": {d.RejectionReason}";
                            deducDescList.Add(msg);
                        }
                    }
                }
                if (deducDescList.Any())
                {
                    desc = desc + (desc == attendance.Status.ToString() ? " | " : ", ") + string.Join(", ", deducDescList);
                }
                
                worksheet.Cell(row, 12).Value = desc;
            }
            else
            {
                var isWeekend = date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday;
                string dutyCode = isWeekend ? "WE" : "AWP";
                if (periodOvertimes.TryGetValue(date, out var ots) && ots.Any())
                {
                    dutyCode = ots.First().Type;
                }
                worksheet.Cell(row, 4).Value = dutyCode;
                
                string desc = isWeekend ? "Weekend" : "Absent";
                if (periodOvertimes.TryGetValue(date, out var overtimes))
                {
                    var otDesc = string.Join(", ", overtimes.Select(o => $"{o.Type} from {o.StartTime} to {o.EndTime}"));
                    desc = desc + " | " + otDesc;
                }

                var deducDescList = new List<string>();
                if (periodDeductions.TryGetValue(date, out var deducs))
                {
                    foreach (var d in deducs)
                    {
                        if (d.Status == PayrollStatus.Rejected)
                        {
                            var msg = "Deduction Approved";
                            if (!string.IsNullOrWhiteSpace(d.RejectionReason)) msg += $": {d.RejectionReason}";
                            deducDescList.Add(msg);
                        }
                        else if (d.Status == PayrollStatus.Waived)
                        {
                            var msg = "Deduction Rejected/Waived";
                            if (!string.IsNullOrWhiteSpace(d.RejectionReason)) msg += $": {d.RejectionReason}";
                            deducDescList.Add(msg);
                        }
                    }
                }
                if (deducDescList.Any())
                {
                    desc = desc + (desc == (isWeekend ? "Weekend" : "Absent") ? " | " : ", ") + string.Join(", ", deducDescList);
                }

                worksheet.Cell(row, 12).Value = desc;
            }
        }

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportAllTimesheetsAsync(DateOnly startDate, DateOnly endDate, string templatePath)
    {
        using var workbook = new XLWorkbook(templatePath);
        var templateSheet = workbook.Worksheet(1);
        templateSheet.Name = "Template_Hidden";
        templateSheet.Hide();

        var employees = (await _employeeRepository.GetAllAsync()).Where(e => !e.IsDeleted).ToList();
        
        var allAttendances = await _attendanceRepository.GetAttendancesByDateRangeAsync(startDate, endDate);
        var attendancesByEmployee = allAttendances.GroupBy(a => a.EmployeeId).ToDictionary(g => g.Key, g => g.ToList());

        foreach (var employee in employees)
        {
            var baseSheetName = $"{employee.FirstName}_{employee.EmployeeCode}".Replace("/", "_").Replace("\\", "_");
            if (baseSheetName.Length > 28) baseSheetName = baseSheetName.Substring(0, 28);
            
            string sheetName = baseSheetName;
            int attempt = 1;
            while (workbook.Worksheets.Contains(sheetName))
            {
                sheetName = $"{baseSheetName}_{attempt}";
                attempt++;
            }
            
            var worksheet = templateSheet.CopyTo(sheetName);
            worksheet.Unhide();

            worksheet.Cell(6, 3).Value = $"{employee.FirstName} {employee.LastName}"; // Col C, Row 6: NAME
            worksheet.Cell(6, 7).Value = employee.Position?.Name ?? ""; // Col G, Row 6: POSITION
            worksheet.Cell(6, 10).Value = employee.Department?.Name ?? ""; // Col J, Row 6: DEP.
            
            worksheet.Cell(7, 3).Value = startDate.Month; // Col C, Row 7: MONTH
            worksheet.Cell(7, 7).Value = startDate.Year; // Col G, Row 7: YEAR
            worksheet.Cell(8, 3).Value = $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}"; // Col C, Row 8: PERIOD

            var attendances = attendancesByEmployee.GetValueOrDefault(employee.Id, new List<Attendance>());
            var periodAttendances = attendances
                .GroupBy(a => a.Date)
                .ToDictionary(g => g.Key, g => g.First());

            var overtimeRequests = await _overtimeRepository.GetByEmployeeAsync(employee.Id);
            var periodOvertimes = overtimeRequests
                .Where(o => o.Date >= startDate && o.Date <= endDate && o.Status == "Approved")
                .GroupBy(o => o.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            var deductions = await _deductionRepository.GetByEmployeeAsync(employee.Id);
            var periodDeductions = deductions
                .Where(d => d.RelatedAttendance != null && d.RelatedAttendance.Date >= startDate && d.RelatedAttendance.Date <= endDate && (d.Status == PayrollStatus.Waived || d.Status == PayrollStatus.Rejected))
                .GroupBy(d => d.RelatedAttendance.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            int startRow = 13;
            
            // 1. Clear all 31 days (rows 13 to 43) contents so no old template data is left
            for (int i = 0; i < 31; i++)
            {
                var r = startRow + i;
                // Do NOT clear columns 2 (Day) and 3 (Date) to preserve template formulas
                worksheet.Cell(r, 4).Clear(XLClearOptions.Contents); // Duty Code
                worksheet.Cell(r, 5).Clear(XLClearOptions.Contents); // Time In
                worksheet.Cell(r, 6).Clear(XLClearOptions.Contents); // Time Out
                worksheet.Cell(r, 12).Clear(XLClearOptions.Contents); // Description
            }

            int daysDiff = (endDate.DayNumber - startDate.DayNumber) + 1;
            if (daysDiff > 31) daysDiff = 31; // Prevent going beyond standard template

            for (int i = 0; i < daysDiff; i++)
            {
                var date = startDate.AddDays(i);
                var row = startRow + i; // Write sequentially starting from row 13
                
                // Overwrite columns 2 (Day Name) and 3 (Date) with actual values
                worksheet.Cell(row, 2).Value = date.ToString("dddd"); // e.g. Monday
                worksheet.Cell(row, 3).Value = date.ToString("dd-MMM-yy"); // e.g. 30-Jun-26

                if (periodAttendances.TryGetValue(date, out var attendance))
                {
                    string dutyCode = "OD";
                    if (periodOvertimes.TryGetValue(date, out var ots) && ots.Any())
                    {
                        dutyCode = ots.First().Type;
                    }
                    worksheet.Cell(row, 4).Value = dutyCode; // Duty Code
                    
                    if (attendance.CheckIn.HasValue)
                        worksheet.Cell(row, 5).Value = attendance.CheckIn.Value.ToTimeSpan(); 
                    
                    if (attendance.CheckOut.HasValue)
                        worksheet.Cell(row, 6).Value = attendance.CheckOut.Value.ToTimeSpan();
                        
                    string desc = attendance.Status.ToString();

                    // Overwrite formulas if Present (approved correction)
                    if (attendance.Status == MWMS.Domain.Enums.AttendanceStatus.Present)
                    {
                        worksheet.Cell(row, 7).Value = TimeSpan.Zero;
                        worksheet.Cell(row, 8).Value = TimeSpan.Zero;
                    }

                    if (periodOvertimes.TryGetValue(date, out var overtimes))
                    {
                        var otDesc = string.Join(", ", overtimes.Select(o => $"{o.Type} from {o.StartTime} to {o.EndTime}"));
                        desc = desc + " | " + otDesc;
                    }
                    
                    var deducDescList = new List<string>();
                    if (periodDeductions.TryGetValue(date, out var deducs))
                    {
                        foreach (var d in deducs)
                        {
                        if (d.Status == PayrollStatus.Rejected)
                        {
                            var msg = "Deduction Approved";
                            if (!string.IsNullOrWhiteSpace(d.RejectionReason)) msg += $": {d.RejectionReason}";
                            deducDescList.Add(msg);
                        }
                        else if (d.Status == PayrollStatus.Waived)
                        {
                            var msg = "Deduction Rejected/Waived";
                            if (!string.IsNullOrWhiteSpace(d.RejectionReason)) msg += $": {d.RejectionReason}";
                            deducDescList.Add(msg);
                        }
                        }
                    }
                    if (deducDescList.Any())
                    {
                        desc = desc + (desc == attendance.Status.ToString() ? " | " : ", ") + string.Join(", ", deducDescList);
                    }
                    
                    worksheet.Cell(row, 12).Value = desc;
                }
                else
                {
                    var isWeekend = date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday;
                    string dutyCode = isWeekend ? "WE" : "AWP";
                    if (periodOvertimes.TryGetValue(date, out var ots) && ots.Any())
                    {
                        dutyCode = ots.First().Type;
                    }
                    worksheet.Cell(row, 4).Value = dutyCode;
                    
                    string desc = isWeekend ? "Weekend" : "Absent";
                    if (periodOvertimes.TryGetValue(date, out var overtimes))
                    {
                        var otDesc = string.Join(", ", overtimes.Select(o => $"{o.Type} from {o.StartTime} to {o.EndTime}"));
                        desc = desc + " | " + otDesc;
                    }

                    var deducDescList = new List<string>();
                    if (periodDeductions.TryGetValue(date, out var deducs))
                    {
                        foreach (var d in deducs)
                        {
                        if (d.Status == PayrollStatus.Rejected)
                        {
                            var msg = "Deduction Approved";
                            if (!string.IsNullOrWhiteSpace(d.RejectionReason)) msg += $": {d.RejectionReason}";
                            deducDescList.Add(msg);
                        }
                        else if (d.Status == PayrollStatus.Waived)
                        {
                            var msg = "Deduction Rejected/Waived";
                            if (!string.IsNullOrWhiteSpace(d.RejectionReason)) msg += $": {d.RejectionReason}";
                            deducDescList.Add(msg);
                        }
                        }
                    }
                    if (deducDescList.Any())
                    {
                        desc = desc + (desc == (isWeekend ? "Weekend" : "Absent") ? " | " : ", ") + string.Join(", ", deducDescList);
                    }

                    worksheet.Cell(row, 12).Value = desc;
                }
            }
        }

        if (employees.Any())
        {
            templateSheet.Delete();
        }
        else
        {
            templateSheet.Unhide();
            templateSheet.Name = "No Data";
        }

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<IEnumerable<AttendanceResponseDto>> SearchAttendanceAsync(AttendanceFilterDto filter)
    {
        var attendances = await _attendanceRepository.SearchAttendancesAsync(filter);
        return attendances.Select(a => new AttendanceResponseDto
        {
            EmployeeId = a.EmployeeId,
            EmployeeCode = a.Employee.EmployeeCode,
            EmployeeName = $"{a.Employee.FirstName} {a.Employee.LastName}",
            Date = a.Date,
            CheckIn = a.CheckIn,
            CheckOut = a.CheckOut,
            Status = a.Status.ToString(),
            WorkedHours = a.WorkedHours,
            LateMinutes = a.LateMinutes,
            EarlyLeaveMinutes = a.EarlyLeaveMinutes,
            OvertimeMinutes = a.OvertimeMinutes
        });
    }

    public async Task<byte[]> ExportReportsAsync(AttendanceFilterDto filter, string format)
    {
        var attendances = (await SearchAttendanceAsync(filter)).ToList();

        if (format.Equals("csv", StringComparison.OrdinalIgnoreCase))
        {
            var sb = new StringBuilder();
            sb.AppendLine("Date,Employee ID,Name,Check In,Check Out,Status,Worked Hours,Late Min,Early Leave Min,Overtime Min");
            foreach (var a in attendances)
            {
                sb.AppendLine($"{a.Date:yyyy-MM-dd},{a.EmployeeId},\"{a.EmployeeName}\",{a.CheckIn?.ToString("hh:mm tt") ?? "--:--"},{a.CheckOut?.ToString("hh:mm tt") ?? "--:--"},{a.Status},{Math.Round(a.WorkedHours, 2)},{a.LateMinutes},{a.EarlyLeaveMinutes},{a.OvertimeMinutes}");
            }
            return Encoding.UTF8.GetBytes(sb.ToString());
        }
        else if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
        {
            QuestPDF.Settings.License = LicenseType.Community;
            
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Text("Attendance Report")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1); // Date
                            columns.RelativeColumn(2); // Name
                            columns.RelativeColumn(1); // ID
                            columns.RelativeColumn(1); // Check In
                            columns.RelativeColumn(1); // Check Out
                            columns.RelativeColumn(1); // Status
                            columns.RelativeColumn(1); // Worked
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Date").SemiBold();
                            header.Cell().Text("Name").SemiBold();
                            header.Cell().Text("ID").SemiBold();
                            header.Cell().Text("Check In").SemiBold();
                            header.Cell().Text("Check Out").SemiBold();
                            header.Cell().Text("Status").SemiBold();
                            header.Cell().Text("Worked").SemiBold();
                        });

                        foreach (var a in attendances)
                        {
                            table.Cell().Text(a.Date.ToString("yyyy-MM-dd"));
                            table.Cell().Text(a.EmployeeName);
                            table.Cell().Text(a.EmployeeId.ToString());
                            table.Cell().Text(a.CheckIn?.ToString("hh:mm tt") ?? "--:--");
                            table.Cell().Text(a.CheckOut?.ToString("hh:mm tt") ?? "--:--");
                            table.Cell().Text(a.Status);
                            table.Cell().Text(Math.Round(a.WorkedHours, 2).ToString());
                        }
                    });
                });
            });

            using var ms = new MemoryStream();
            document.GeneratePdf(ms);
            return ms.ToArray();
        }
        else // default to Excel
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Attendance Report");

            worksheet.Cell(1, 1).Value = "Date";
            worksheet.Cell(1, 2).Value = "Employee ID";
            worksheet.Cell(1, 3).Value = "Name";
            worksheet.Cell(1, 4).Value = "Check In";
            worksheet.Cell(1, 5).Value = "Check Out";
            worksheet.Cell(1, 6).Value = "Status";
            worksheet.Cell(1, 7).Value = "Worked Hours";
            worksheet.Cell(1, 8).Value = "Late Min";
            worksheet.Cell(1, 9).Value = "Early Min";
            worksheet.Cell(1, 10).Value = "Overtime Min";

            worksheet.Range("A1:J1").Style.Font.Bold = true;

            int row = 2;
            foreach (var a in attendances)
            {
                worksheet.Cell(row, 1).Value = a.Date.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 2).Value = a.EmployeeId;
                worksheet.Cell(row, 3).Value = a.EmployeeName;
                worksheet.Cell(row, 4).Value = a.CheckIn?.ToString("hh:mm tt") ?? "--:--";
                worksheet.Cell(row, 5).Value = a.CheckOut?.ToString("hh:mm tt") ?? "--:--";
                worksheet.Cell(row, 6).Value = a.Status;
                worksheet.Cell(row, 7).Value = Math.Round(a.WorkedHours, 2);
                worksheet.Cell(row, 8).Value = a.LateMinutes;
                worksheet.Cell(row, 9).Value = a.EarlyLeaveMinutes;
                worksheet.Cell(row, 10).Value = a.OvertimeMinutes;
                row++;
            }
            
            worksheet.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }
    }

    public async Task<IEnumerable<SubmittedTimesheetDto>> GetSubmittedTimesheetsAsync()
    {
        var saveDirectory = @"D:\MWMS\SubmittedTimesheets";
        if (!Directory.Exists(saveDirectory)) return new List<SubmittedTimesheetDto>();

        var files = Directory.GetFiles(saveDirectory, "*.xlsx")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.CreationTime)
            .ToList();

        var result = new List<SubmittedTimesheetDto>();
        foreach (var file in files)
        {
            int employeeId = 0;
            string empName = "Unknown";
            
            // Expected formats: EMP-113.xlsx or 221_20260712...xlsx
            if (file.Name.StartsWith("EMP-"))
            {
                var codeWithExt = file.Name.Substring(4);
                var code = codeWithExt.Replace(".xlsx", "");
                if (code.Contains('_'))
                {
                    code = code.Split('_')[0];
                }
                var emp = await _employeeRepository.GetByEmployeeCodeAsync(code);
                if (emp == null) emp = await _employeeRepository.GetByEmployeeCodeAsync($"EMP-SYNC-{code}");
                
                if (emp != null)
                {
                    employeeId = emp.Id;
                    empName = $"{emp.FirstName} {emp.LastName}";
                }
                else if (int.TryParse(code, out var id))
                {
                    // Fallback in case the old logic saved internal ID instead of Code
                    emp = await _employeeRepository.GetByIdAsync(id);
                    if (emp != null)
                    {
                        employeeId = emp.Id;
                        empName = $"{emp.FirstName} {emp.LastName}";
                    }
                }
            }
            else
            {
                var parts = file.Name.Split('_');
                if (parts.Length > 0 && int.TryParse(parts[0], out employeeId))
                {
                    var emp = await _employeeRepository.GetByIdAsync(employeeId);
                    if (emp != null) empName = $"{emp.FirstName} {emp.LastName}";
                }
            }

            result.Add(new SubmittedTimesheetDto
            {
                FileName = file.Name,
                EmployeeId = employeeId,
                EmployeeName = empName,
                SubmittedAt = file.CreationTime,
                FileSizeBytes = file.Length
            });
        }
        
        return result;
    }

    public Task<byte[]> GetSubmittedTimesheetFileAsync(string fileName)
    {
        var saveDirectory = @"D:\MWMS\SubmittedTimesheets";
        var filePath = Path.Combine(saveDirectory, fileName);
        
        if (!File.Exists(filePath)) throw new FileNotFoundException("The requested timesheet was not found.");
        
        return File.ReadAllBytesAsync(filePath);
    }

    public async Task<byte[]> DownloadAllSubmittedTimesheetsAsync()
    {
        var saveDirectory = @"D:\MWMS\SubmittedTimesheets";
        if (!Directory.Exists(saveDirectory)) return Array.Empty<byte>();

        var files = Directory.GetFiles(saveDirectory, "*.xlsx");
        if (files.Length == 0) return Array.Empty<byte>();

        using var memoryStream = new MemoryStream();
        using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
        {
            foreach (var file in files)
            {
                var entryName = Path.GetFileName(file);
                var entry = archive.CreateEntry(entryName);
                using var entryStream = entry.Open();
                using var fileStream = File.OpenRead(file);
                await fileStream.CopyToAsync(entryStream);
            }
        }
        return memoryStream.ToArray();
    }

    public Task DeleteSubmittedTimesheetAsync(string fileName)
    {
        var saveDirectory = @"D:\MWMS\SubmittedTimesheets";
        var filePath = Path.Combine(saveDirectory, fileName);
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        
        return Task.CompletedTask;
    }
}