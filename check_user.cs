using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MWMS.Persistence.Context;
using MWMS.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

class Program
{
    static void Main()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer("Server=.;Database=MWMSDb;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False");

        using (var db = new AppDbContext(optionsBuilder.Options))
        {
            var user = db.Users.FirstOrDefault(u => u.Username == "EMP-SYNC-8");
            if (user != null)
            {
                Console.WriteLine($"Found user: {user.Username}, Email: {user.Email}, Role: {user.Role}");
            }
            else
            {
                Console.WriteLine("User EMP-SYNC-8 not found");
            }
            
            var user88 = db.Users.FirstOrDefault(u => u.Username.Contains("88"));
            if (user88 != null)
            {
                Console.WriteLine($"Found user88: {user88.Username}, Email: {user88.Email}, Role: {user88.Role}");
            }
            else
            {
                Console.WriteLine("User with 88 in username not found");
            }
        }
    }
}
