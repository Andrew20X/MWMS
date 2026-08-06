using System;
using System.Linq;
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
            var date = DateOnly.FromDateTime(DateTime.Today);
            var todayAttendances = db.Attendances.Where(a => a.Date == date).ToList();
            Console.WriteLine($"Found {todayAttendances.Count} attendances for today ({date}).");
            
            foreach (var a in todayAttendances)
            {
                Console.WriteLine($"EmpId: {a.EmployeeId}, CheckIn: {a.CheckIn}, CheckOut: {a.CheckOut}");
            }
            
            var allRaw = db.RawAttendanceLogs.Where(l => EF.Functions.DateDiffDay(l.PunchTime, DateTime.Now) == 0).ToList();
            Console.WriteLine($"Found {allRaw.Count} raw logs for today.");
            foreach (var r in allRaw)
            {
                Console.WriteLine($"EmpId: {r.EmployeeId}, PunchTime: {r.PunchTime}, Processed: {r.IsProcessed}");
            }
        }
    }
}
