using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using MWMS.Application.DTOs;
using MWMS.Application.DTOs.Attendance;

namespace MWMS.Application.Interfaces;

public interface IAttendanceService
{
    Task DeleteMyAttendanceAsync(int employeeId);
    Task DeleteAllRawAttendanceAsync();
    Task<CheckInResponseDto> CheckInAsync(int employeeId);
    Task<AttendanceResponseDto?> CheckOutAsync(int employeeId);
    Task<IEnumerable<AttendanceResponseDto>> GetTodayAttendanceAsync();
    Task<IEnumerable<AttendanceResponseDto>> GetRecentAttendanceAsync(int limit = 50);
    Task<IEnumerable<AttendanceResponseDto>> GetEmployeeAttendanceAsync(int employeeId);
    Task<int> ImportTimesheetAsync(Stream excelStream, int? expectedEmployeeId = null);
    Task<byte[]> ExportEmployeeTimesheetAsync(int employeeId, DateOnly startDate, DateOnly endDate, string templatePath);
    Task<byte[]> ExportAllTimesheetsAsync(DateOnly startDate, DateOnly endDate, string templatePath);
    Task<IEnumerable<AttendanceResponseDto>> SearchAttendanceAsync(AttendanceFilterDto filter);
    Task<byte[]> ExportReportsAsync(AttendanceFilterDto filter, string format);
    Task<IEnumerable<SubmittedTimesheetDto>> GetSubmittedTimesheetsAsync();
    Task<byte[]> GetSubmittedTimesheetFileAsync(string fileName);
    Task<byte[]> DownloadAllSubmittedTimesheetsAsync();
    Task DeleteSubmittedTimesheetAsync(string fileName);
}