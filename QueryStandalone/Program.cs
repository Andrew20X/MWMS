using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MWMS.Persistence.Context;
using MWMS.Infrastructure.Services;
using MWMS.Application.Interfaces;
using MWMS.Persistence.Repositories;
using MWMS.Domain.Entities;
using MWMS.Infrastructure;

namespace QueryStandalone
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer("Server=.\\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;"));
                
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IAttendanceRepository, AttendanceRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IAttendanceEngineService, AttendanceEngineService>();

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Mark a few logs as unprocessed to test
            var logsToProcess = db.RawAttendanceLogs.Where(l => l.EmployeeId == 341).ToList();
            foreach(var log in logsToProcess) {
                log.IsProcessed = false;
            }
            db.SaveChanges();
            
            Console.WriteLine($"Marked {logsToProcess.Count} logs as unprocessed.");

            var engine = scope.ServiceProvider.GetRequiredService<IAttendanceEngineService>();
            await engine.ProcessUnprocessedLogsAsync();
            
            var attendance = db.Attendances.FirstOrDefault(a => a.EmployeeId == 341 && a.Date == new DateOnly(2026, 7, 27));
            if (attendance != null)
            {
                Console.WriteLine($"EmpId: 341, Date: 7/27, CheckIn: {attendance.CheckIn}, CheckOut: {attendance.CheckOut}");
            }
        }
    }
}
