using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Attendance_ZKTeco_Service.Models;
using MWMS.Domain.Entities;
using MWMS.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace MWMS.API.Services
{
    public interface IZKTecoService
    {
        Task<int> FetchLogsAsync(string ipAddress, int port, int machineNumber, DateTime startDate, DateTime endDate);
    }

    public class ZKLogItem
    {
        public string? deviceUserId { get; set; }
        public DateTime recordTime { get; set; }
    }

    public class ZKTecoService : IZKTecoService
    {
        private readonly AppDbContext _context;

        public ZKTecoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> FetchLogsAsync(string ipAddress, int port, int machineNumber, DateTime startDate, DateTime endDate)
        {
            var scriptPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "fetch_zkteco.js");
            var outputPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "zk_logs.json");

            if (!System.IO.File.Exists(scriptPath))
            {
                throw new Exception($"Node script not found at {scriptPath}");
            }

            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "node",
                Arguments = $"\"{scriptPath}\" {ipAddress} {port} \"{outputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new System.Diagnostics.Process { StartInfo = processStartInfo };
            process.Start();
            
            // Wait up to 30 seconds for the node script to fetch 50k+ logs
            bool completed = process.WaitForExit(30000);
            
            if (!completed)
            {
                process.Kill();
                throw new Exception("Connection to device timed out while fetching logs via Node.");
            }

            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                throw new Exception($"Failed to fetch logs: {error}");
            }

            if (!System.IO.File.Exists(outputPath))
            {
                return 0; // No logs generated
            }

            var jsonContent = await System.IO.File.ReadAllTextAsync(outputPath);
            var logs = System.Text.Json.JsonSerializer.Deserialize<List<ZKLogItem>>(jsonContent);
            
            // Clean up
            System.IO.File.Delete(outputPath);

            if (logs == null || !logs.Any())
            {
                return 0;
            }

            int importedCount = 0;
            
            var employees = await _context.Employees.ToListAsync();
            
            var existingLogs = await _context.RawAttendanceLogs
                .Where(x => x.PunchTime >= startDate && x.PunchTime <= endDate)
                .ToListAsync();

            var newLogs = new List<RawAttendanceLog>();

            foreach (var log in logs)
            {
                try
                {
                    DateTime recordTime = log.recordTime.ToLocalTime();
                    
                    if (recordTime >= startDate && recordTime <= endDate)
                    {
                        string employeeCodeStr = log.deviceUserId ?? "";
                        int devUserId = 0;
                        int.TryParse(log.deviceUserId, out devUserId);

                        // Check if duplicate
                        if (existingLogs.Any(x => x.Employee != null && 
                            (x.Employee.DeviceUserId == devUserId || 
                             x.Employee.EmployeeCode == employeeCodeStr ||
                             x.Employee.EmployeeCode == $"EMP-SYNC-{employeeCodeStr}" ||
                             x.Employee.EmployeeCode == $"EMP-{employeeCodeStr}") 
                             && x.PunchTime == recordTime))
                        {
                            continue;
                        }

                        var emp = employees
                            .Where(e => 
                                (devUserId != 0 && e.DeviceUserId == devUserId) || 
                                e.EmployeeCode == employeeCodeStr ||
                                e.EmployeeCode == $"EMP-SYNC-{employeeCodeStr}" ||
                                e.EmployeeCode == $"EMP-{employeeCodeStr}")
                            .OrderByDescending(e => !e.IsDeleted)
                            .ThenByDescending(e => e.IsActive)
                            .ThenByDescending(e => e.Id)
                            .FirstOrDefault();
                        if (emp != null)
                        {
                            newLogs.Add(new RawAttendanceLog
                            {
                                EmployeeId = emp.Id,
                                PunchTime = recordTime,
                                DeviceId = ipAddress,
                                IsProcessed = false,
                                CreatedAt = DateTime.UtcNow
                            });
                            importedCount++;
                        }
                    }
                }
                catch
                {
                    // Ignore parse errors
                }
            }

            if (newLogs.Any())
            {
                _context.RawAttendanceLogs.AddRange(newLogs);
                await _context.SaveChangesAsync();
            }

            return importedCount;
        }
    }
}
