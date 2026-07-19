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
            var loay = db.Employees.Include(e => e.Position).FirstOrDefault(e => e.FirstName.Contains("Loay"));
            if (loay != null)
            {
                Console.WriteLine($"Found Loay: {loay.FirstName} {loay.LastName}, Position: {loay.Position?.Name}");
            }
            else
            {
                Console.WriteLine("Loay not found in Employees table.");
            }
            
            var user = db.Users.FirstOrDefault(u => u.FullName.Contains("Loay"));
            if (user != null)
            {
                Console.WriteLine($"Found Loay in Users: {user.FullName}, Role: {user.Role}, Username: {user.Username}");
            }
        }
    }
}
