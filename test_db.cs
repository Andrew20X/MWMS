using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MWMS.Persistence.Context;
using System.IO;

class Program
{
    static void Main()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer("Server=.;Database=MWMSDb;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False");

        using (var db = new AppDbContext(optionsBuilder.Options))
        {
            var empCount = db.Employees.Count();
            var shiftCount = db.Shifts.Count();
            var rawCount = db.RawAttendanceLogs.Count();
            var unprocessedCount = db.RawAttendanceLogs.Count(r => !r.IsProcessed);
            var attendanceCount = db.Attendances.Count();

            var employeesNoShift = db.Employees.Where(e => e.ShiftId == null).Count();
            var employee88Shift = db.Employees.Where(e => e.EmployeeCode == "88").Select(e => e.ShiftId).FirstOrDefault();

            Console.WriteLine($"Total Employees: {empCount}");
            Console.WriteLine($"Total Shifts: {shiftCount}");
            Console.WriteLine($"Employees without Shift: {employeesNoShift}");
            Console.WriteLine($"Employee 88 ShiftId: {employee88Shift}");
            
            Console.WriteLine($"Raw logs: {rawCount}");
            Console.WriteLine($"Unprocessed logs: {unprocessedCount}");
            Console.WriteLine($"Attendances: {attendanceCount}");
        }
    }
}
