using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MWMS.Application.Interfaces;

namespace MWMS.API.Controllers;

[ApiController]
//[Authorize]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn()
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;

        if (employeeIdClaim == null)
            return Unauthorized();

        var employeeId = int.Parse(employeeIdClaim);

        var result = await _attendanceService.CheckInAsync(employeeId);

        return Ok(result);
    }

    [HttpPost("check-out")]
    public async Task<IActionResult> CheckOut()
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;

        if (employeeIdClaim == null)
            return Unauthorized();

        var employeeId = int.Parse(employeeIdClaim);

        var result = await _attendanceService.CheckOutAsync(employeeId);

        return Ok(result);
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetTodayAttendance()
    {
        var result = await _attendanceService.GetTodayAttendanceAsync();

        return Ok(result);
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentAttendance([FromQuery] int limit = 50)
    {
        var result = await _attendanceService.GetRecentAttendanceAsync(limit);

        return Ok(result);
    }

    [HttpGet("employee/{employeeId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetEmployeeAttendance(int employeeId)
    {
        var result = await _attendanceService.GetEmployeeAttendanceAsync(employeeId);

        return Ok(result);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyAttendance()
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        if (employeeIdClaim == null) return Ok(Array.Empty<object>());

        var employeeId = int.Parse(employeeIdClaim);
        var result = await _attendanceService.GetEmployeeAttendanceAsync(employeeId);

        return Ok(result);
    }

    [HttpGet("pending-resolution")]
    public async Task<IActionResult> GetPendingResolution([FromServices] MWMS.Persistence.Context.AppDbContext db)
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        if (employeeIdClaim == null) return Unauthorized();

        var employeeId = int.Parse(employeeIdClaim);
        
        var pendingAbsences = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
            System.Linq.Queryable.Where(db.Attendances, a => 
                a.EmployeeId == employeeId && 
                a.Status == MWMS.Domain.Enums.AttendanceStatus.Absent && 
                a.AbsenceResolutionStatus == MWMS.Domain.Enums.AbsenceResolutionStatus.PendingResolution)
        );

        return Ok(pendingAbsences);
    }

    [HttpDelete("me")]
    public async Task<IActionResult> DeleteMyAttendance([FromServices] MWMS.Persistence.Context.AppDbContext db)
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        if (employeeIdClaim == null) return Unauthorized();

        var employeeId = int.Parse(employeeIdClaim);
        await _attendanceService.DeleteMyAttendanceAsync(employeeId);

        var rawLogs = db.RawAttendanceLogs.Where(r => r.EmployeeId == employeeId);
        db.RawAttendanceLogs.RemoveRange(rawLogs);
        await db.SaveChangesAsync();

        return Ok(new { message = "Attendance data and raw logs cleared successfully." });
    }

    [HttpGet("debug-attendances/{code}")]
    [AllowAnonymous]
    public IActionResult DebugAttendances(string code, [FromServices] MWMS.Persistence.Context.AppDbContext db)
    {
        var emp = db.Employees.FirstOrDefault(e => e.EmployeeCode == code || e.EmployeeCode == $"EMP-SYNC-{code}");
        if (emp == null) return NotFound("Employee not found");
        
        var attendances = db.Attendances
            .Where(a => a.EmployeeId == emp.Id)
            .OrderByDescending(a => a.Date)
            .Take(10)
            .Select(a => new {
                Date = a.Date.ToString("yyyy-MM-dd"),
                CheckIn = a.CheckIn.HasValue ? a.CheckIn.Value.ToString("HH:mm") : null,
                CheckOut = a.CheckOut.HasValue ? a.CheckOut.Value.ToString("HH:mm") : null,
                Status = a.Status.ToString(),
                LateMinutes = a.LateMinutes
            })
            .ToList();
            
        return Ok(attendances);
    }

    [HttpGet("debug-device-raw")]
    [AllowAnonymous]
    public async Task<IActionResult> DebugDeviceRaw()
    {
        var scriptPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "fetch_zkteco.js");
        var outputPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "zk_logs.json");

        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "node",
            Arguments = $"\"{scriptPath}\" 10.10.100.102 4370 \"{outputPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new System.Diagnostics.Process { StartInfo = processStartInfo };
        process.Start();
        process.WaitForExit(30000);

        if (!System.IO.File.Exists(outputPath)) return NotFound("No logs file created");

        var jsonContent = await System.IO.File.ReadAllTextAsync(outputPath);
        return Content(jsonContent, "application/json");
    }

    [HttpGet("debug-engy")]
    [AllowAnonymous]
    public IActionResult DebugEngy([FromServices] MWMS.Persistence.Context.AppDbContext db)
    {
        var emp = db.Employees.FirstOrDefault(e => e.FirstName.Contains("Engy") || e.EmployeeCode.Contains("109"));
        if (emp == null) return NotFound("Engy not found");

        var attendances = db.Attendances.Where(a => a.EmployeeId == emp.Id).ToList();
        return Ok(new { Employee = emp, Attendances = attendances });
    }

    [HttpGet("debug-raw/{employeeId}")]
    [AllowAnonymous]
    public IActionResult DebugRaw(int employeeId, [FromServices] MWMS.Persistence.Context.AppDbContext db)
    {
        var rawLogs = db.RawAttendanceLogs.Where(r => r.EmployeeId == employeeId).OrderByDescending(r => r.PunchTime).ToList();
        return Ok(rawLogs);
    }

    [HttpGet("debug-attendances-full/{employeeId}")]
    [AllowAnonymous]
    public IActionResult DebugAttendancesFull(int employeeId, [FromServices] MWMS.Persistence.Context.AppDbContext db)
    {
        var attendances = db.Attendances.Where(a => a.EmployeeId == employeeId).OrderByDescending(a => a.Date).ToList();
        return Ok(attendances);
    }

    [HttpPost("import")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ImportTimesheet(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required.");

        if (!file.FileName.EndsWith(".xlsx"))
            return BadRequest("Only .xlsx files are supported.");

        using var stream = file.OpenReadStream();
        var importedCount = await _attendanceService.ImportTimesheetAsync(stream);

        return Ok(new { message = $"Successfully imported {importedCount} attendance records." });
    }

    [HttpGet("export/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExportTimesheetsAll([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var templatePath = @"D:\MWMS\New Format Updated.xlsx";
            
            if (!System.IO.File.Exists(templatePath))
            {
                return BadRequest("Excel template not found on the server.");
            }

            var fileBytes = await _attendanceService.ExportAllTimesheetsAsync(
                DateOnly.FromDateTime(startDate), 
                DateOnly.FromDateTime(endDate), 
                templatePath
            );
            
            var fileName = $"Timesheets_All_{startDate:yyyyMMdd}_to_{endDate:yyyyMMdd}.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return StatusCode(500, $"An error occurred during export: {ex.Message}");
        }
    }

    [HttpGet("export/me")]
    public async Task<IActionResult> ExportTimesheetMe([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
            if (employeeIdClaim == null) return Unauthorized();

            var employeeId = int.Parse(employeeIdClaim);
            var templatePath = @"D:\MWMS\New Format Updated.xlsx";
            
            if (!System.IO.File.Exists(templatePath))
            {
                return BadRequest("Excel template not found on the server.");
            }

            var fileBytes = await _attendanceService.ExportEmployeeTimesheetAsync(
                employeeId,
                DateOnly.FromDateTime(startDate), 
                DateOnly.FromDateTime(endDate), 
                templatePath
            );
            
            var monthName = startDate.ToString("MMMM");
            var monthNum = startDate.Month.ToString("D2");
            var year = startDate.Year;
            var fileName = $"MyTimesheet_{monthName}_{monthNum}_{year}.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return StatusCode(500, $"An error occurred during export: {ex.Message}");
        }
    }

    [HttpPost("import/me")]
    public async Task<IActionResult> ImportTimesheetMe(IFormFile file)
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        if (employeeIdClaim == null) return Unauthorized();
        
        var employeeId = int.Parse(employeeIdClaim);

        if (file == null || file.Length == 0)
            return BadRequest("File is required.");

        if (!file.FileName.EndsWith(".xlsx"))
            return BadRequest("Only .xlsx files are supported.");

        try
        {
            using var stream = file.OpenReadStream();
            var importedCount = await _attendanceService.ImportTimesheetAsync(stream, employeeId);

            return Ok(new { message = $"Successfully imported {importedCount} attendance records." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred during import: {ex.Message}");
        }
    }

    [HttpPost("upload-final/me")]
    public async Task<IActionResult> UploadFinalTimesheet(List<IFormFile> files)
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        if (employeeIdClaim == null) return Unauthorized();

        if (files == null || files.Count == 0)
            return BadRequest("File is required.");

        var deadline = GetCurrentDeadline();
        if (deadline.HasValue && DateTime.Now > deadline.Value)
        {
            return BadRequest($"The submission deadline ({deadline.Value:g}) has passed. You can no longer submit timesheets.");
        }

        foreach (var file in files)
        {
            if (!file.FileName.EndsWith(".xlsx"))
                return BadRequest("Only .xlsx files are supported.");
        }

        try
        {
            var saveDirectory = @"D:\MWMS\SubmittedTimesheets";
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }

            var employeeRepository = HttpContext.RequestServices.GetRequiredService<MWMS.Application.Interfaces.IEmployeeRepository>();
            var employee = await employeeRepository.GetByIdAsync(int.Parse(employeeIdClaim));
            var employeeCode = employee?.EmployeeCode ?? employeeIdClaim;

            var baseCode = employeeCode.StartsWith("EMP-") ? employeeCode : $"EMP-{employeeCode}";
            
            foreach (var file in files)
            {
                var originalName = Path.GetFileName(file.FileName);
                // Ensure the file name is unique to the employee but keeps their original file name context
                var fileName = originalName.StartsWith(baseCode) ? originalName : $"{baseCode}_{originalName}";
                var savePath = Path.Combine(saveDirectory, fileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            return Ok(new { message = "Timesheet successfully submitted to HR." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred during upload: {ex.Message}");
        }
    }

    [HttpGet("submitted")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetSubmittedTimesheets([FromServices] MWMS.Persistence.Context.AppDbContext db)
    {
        try
        {
            var result = await _attendanceService.GetSubmittedTimesheetsAsync();
            var comments = db.SubmissionComments.Where(c => !c.IsDeleted).ToList();
            foreach (var item in result)
            {
                var itemComments = comments.Where(c => c.FileName == item.FileName).OrderByDescending(c => c.CreatedAt).ToList();
                item.CommentCount = itemComments.Count;
                item.LatestComment = itemComments.FirstOrDefault()?.CommentText;
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpGet("submitted/download/{fileName}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DownloadSubmittedTimesheet(string fileName)
    {
        try
        {
            var fileBytes = await _attendanceService.GetSubmittedTimesheetFileAsync(fileName);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound("File not found.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpGet("submitted/download-all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DownloadAllSubmittedTimesheets()
    {
        try
        {
            var fileBytes = await _attendanceService.DownloadAllSubmittedTimesheetsAsync();
            if (fileBytes.Length == 0) return NotFound("No submitted timesheets found.");
            
            return File(fileBytes, "application/zip", $"All_Submitted_Timesheets_{DateTime.Now:yyyyMMdd_HHmmss}.zip");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpDelete("submitted/{fileName}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSubmittedTimesheet(string fileName)
    {
        try
        {
            await _attendanceService.DeleteSubmittedTimesheetAsync(fileName);
            return Ok(new { message = "Timesheet deleted successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpDelete("raw/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAllRawAttendance()
    {
        try
        {
            await _attendanceService.DeleteAllRawAttendanceAsync();
            return Ok(new { message = "All raw attendance data cleared successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    public class FetchDeviceRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("fetch-from-device")]
    public async Task<IActionResult> FetchFromDevice([FromBody] FetchDeviceRequest request, [FromServices] MWMS.API.Services.IZKTecoService zkTecoService)
    {
        try
        {
            string ipAddress = "10.10.100.102";
            int port = 4370;
            int machineNumber = 1;

            var endDate = request.EndDate;
            // Since EndDate comes from date picker as midnight, we set it to end of day
            endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59);
            var startDate = request.StartDate;

            int count = await zkTecoService.FetchLogsAsync(ipAddress, port, machineNumber, startDate, endDate);

            var attendanceEngine = HttpContext.RequestServices.GetRequiredService<MWMS.Application.Interfaces.IAttendanceEngineService>();
            await attendanceEngine.ProcessUnprocessedLogsAsync();

            return Ok(new { message = $"Successfully connected to device and imported {count} new records." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Failed to connect or fetch from device: {ex.Message}");
        }
    }

    // --- Settings and My Submissions ---

    private DateTime? GetCurrentDeadline()
    {
        var settingsPath = @"D:\MWMS\submission_settings.json";
        if (System.IO.File.Exists(settingsPath))
        {
            try
            {
                var content = System.IO.File.ReadAllText(settingsPath);
                var obj = System.Text.Json.JsonDocument.Parse(content);
                if (obj.RootElement.TryGetProperty("deadline", out var deadlineEl) && deadlineEl.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    if (DateTime.TryParse(deadlineEl.GetString(), out var d)) return d;
                }
            } catch { }
        }
        return null;
    }

    [HttpGet("settings")]
    public IActionResult GetSettings()
    {
        return Ok(new { deadline = GetCurrentDeadline() });
    }

    public class UpdateSettingsRequest
    {
        public DateTime? Deadline { get; set; }
    }

    [HttpPost("settings")]
    [Authorize(Roles = "Admin")]
    public IActionResult UpdateSettings([FromBody] UpdateSettingsRequest request)
    {
        var settingsPath = @"D:\MWMS\submission_settings.json";
        System.IO.File.WriteAllText(settingsPath, System.Text.Json.JsonSerializer.Serialize(new { deadline = request.Deadline }));
        return Ok(new { message = "Settings updated successfully" });
    }

    [HttpGet("submitted/my-timesheets")]
    public async Task<IActionResult> GetMyTimesheets()
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        if (employeeIdClaim == null) return Unauthorized();
        
        var employeeRepository = HttpContext.RequestServices.GetRequiredService<MWMS.Application.Interfaces.IEmployeeRepository>();
        var employee = await employeeRepository.GetByIdAsync(int.Parse(employeeIdClaim));
        var employeeCode = employee?.EmployeeCode ?? employeeIdClaim;
        var baseCode = employeeCode.StartsWith("EMP-") ? employeeCode : $"EMP-{employeeCode}";

        var saveDirectory = @"D:\MWMS\SubmittedTimesheets";
        if (!Directory.Exists(saveDirectory)) return Ok(new List<object>());

        var files = Directory.GetFiles(saveDirectory)
            .Select(f => new FileInfo(f))
            .Where(f => f.Name.StartsWith(baseCode + "_"))
            .Select(f => new
            {
                fileName = f.Name,
                submittedAt = f.CreationTime,
                size = f.Length
            })
            .OrderByDescending(f => f.submittedAt)
            .ToList();

        return Ok(files);
    }

    [HttpGet("submitted/comments/{fileName}")]
    public async Task<IActionResult> GetSubmissionComments(string fileName, [FromServices] MWMS.Persistence.Context.AppDbContext db)
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (employeeIdClaim == null && roleClaim != "Admin") return Unauthorized();

        var comments = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
            db.SubmissionComments
            .Where(c => c.FileName == fileName)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new {
                c.Id,
                CommentText = c.CommentText.StartsWith("[ADMIN] ") ? c.CommentText.Substring(8) : c.CommentText,
                c.CreatedAt,
                Author = c.CommentText.StartsWith("[ADMIN] ") ? "System Admin" : c.Employee.FirstName + " " + c.Employee.LastName,
                c.EmployeeId
            })
        );

        return Ok(comments);
    }

    public class AddCommentRequest {
        public string CommentText { get; set; }
    }

    [HttpPost("submitted/comments/{fileName}")]
    public async Task<IActionResult> AddSubmissionComment(string fileName, [FromBody] AddCommentRequest request, [FromServices] MWMS.Persistence.Context.AppDbContext db)
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (employeeIdClaim == null && roleClaim != "Admin") return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.CommentText)) return BadRequest("Comment text is required.");

        int finalEmployeeId;

        if (employeeIdClaim != null)
        {
            finalEmployeeId = int.Parse(employeeIdClaim);
        }
        else 
        {
            var parts = fileName.Split('_');
            var empCodePart = parts[0].Replace("EMP-", "");
            var fileOwner = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                db.Employees, e => e.EmployeeCode == empCodePart);
            
            if (fileOwner != null) {
                finalEmployeeId = fileOwner.Id;
                request.CommentText = "[ADMIN] " + request.CommentText;
            } else {
                return BadRequest("Could not determine file owner to attach comment.");
            }
        }

        var comment = new MWMS.Domain.Entities.SubmissionComment
        {
            EmployeeId = finalEmployeeId,
            FileName = fileName,
            CommentText = request.CommentText
        };

        db.SubmissionComments.Add(comment);
        await db.SaveChangesAsync();

        var employee = await db.Employees.FindAsync(finalEmployeeId);
        return Ok(new {
            comment.Id,
            CommentText = comment.CommentText.StartsWith("[ADMIN] ") ? comment.CommentText.Substring(8) : comment.CommentText,
            comment.CreatedAt,
            Author = comment.CommentText.StartsWith("[ADMIN] ") ? "System Admin" : (employee?.FirstName + " " + employee?.LastName),
            comment.EmployeeId
        });
    }

    [HttpDelete("submitted/comments/{commentId}")]
    public async Task<IActionResult> DeleteSubmissionComment(int commentId, [FromServices] MWMS.Persistence.Context.AppDbContext db)
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (employeeIdClaim == null && roleClaim != "Admin") return Unauthorized();

        var comment = await db.SubmissionComments.FindAsync(commentId);
        if (comment == null) return NotFound();

        bool isAdminComment = comment.CommentText.StartsWith("[ADMIN] ");

        if (roleClaim == "Admin")
        {
            if (!isAdminComment) return Forbid("Admin can only delete admin comments.");
        }
        else
        {
            if (isAdminComment) return Forbid("Employees cannot delete admin comments.");
            if (employeeIdClaim != null && comment.EmployeeId.ToString() != employeeIdClaim)
            {
                return Forbid("You can only delete your own comments.");
            }
        }

        db.SubmissionComments.Remove(comment);
        await db.SaveChangesAsync();

        return Ok(new { message = "Comment deleted successfully." });
    }

    [HttpDelete("submitted/my-timesheets/{fileName}")]
    public async Task<IActionResult> DeleteMyTimesheet(string fileName)
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        if (employeeIdClaim == null) return Unauthorized();
        
        var employeeRepository = HttpContext.RequestServices.GetRequiredService<MWMS.Application.Interfaces.IEmployeeRepository>();
        var employee = await employeeRepository.GetByIdAsync(int.Parse(employeeIdClaim));
        var employeeCode = employee?.EmployeeCode ?? employeeIdClaim;
        var baseCode = employeeCode.StartsWith("EMP-") ? employeeCode : $"EMP-{employeeCode}";

        if (!fileName.StartsWith(baseCode + "_"))
        {
            return Forbid("You can only delete your own timesheets.");
        }

        var filePath = Path.Combine(@"D:\MWMS\SubmittedTimesheets", fileName);
        if (!System.IO.File.Exists(filePath)) return NotFound("File not found.");

        try
        {
            System.IO.File.Delete(filePath);
            return Ok(new { message = "Timesheet deleted successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}
