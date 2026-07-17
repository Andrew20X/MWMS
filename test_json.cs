using System;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MWMS.Persistence.Context;

class Program
{
    static void Main()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer("Server=.;Database=MWMSDb;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False");

        using (var db = new AppDbContext(optionsBuilder.Options))
        {
            var attendances = db.Attendances
                .Include(a => a.Employee)
                .Where(a => a.Employee.EmployeeCode == "88")
                .OrderBy(a => a.Date)
                .Take(2)
                .Select(a => new 
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
                })
                .ToList();

            var json = JsonSerializer.Serialize(attendances, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            Console.WriteLine(json);
        }
    }
}
