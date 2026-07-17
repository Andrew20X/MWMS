using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MWMS.Persistence.Context;

namespace TestDb
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer("Server=.\\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;")
                .Options;

            using var db = new AppDbContext(options);
            
            var emp = db.Employees.FirstOrDefault(e => e.EmployeeCode == "88" || e.EmployeeCode == "EMP-SYNC-88");
            if (emp == null) 
            {
                Console.WriteLine("Employee not found.");
                return;
            }

            var attendances = db.Attendances
                .Where(a => a.EmployeeId == emp.Id)
                .OrderBy(a => a.Date)
                .ToList();

            Console.WriteLine($"Found {attendances.Count} attendances for Employee {emp.EmployeeCode}.");
            foreach (var a in attendances)
            {
                Console.WriteLine($"Date: {a.Date}, CheckIn: {a.CheckIn}, CheckOut: {a.CheckOut}, Status: {a.Status}");
            }
        }
    }
}
