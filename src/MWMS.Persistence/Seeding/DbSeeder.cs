using Microsoft.EntityFrameworkCore;
using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;
using MWMS.Persistence.Context;

namespace MWMS.Persistence.Seeding;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher)
    {
        if (!await context.Departments.AnyAsync())
        {
            context.Departments.AddRange(
                new Department { Name = "Administration", Description = "Admin and HR" },
                new Department { Name = "Operations", Description = "Daily operations" },
                new Department { Name = "IT", Description = "Information technology" });
        }

        if (!await context.Positions.AnyAsync())
        {
            context.Positions.AddRange(
                new Position { Name = "Manager" },
                new Position { Name = "Supervisor" },
                new Position { Name = "Staff" });
        }

        if (!await context.Shifts.AnyAsync())
        {
            context.Shifts.AddRange(
                new Shift
                {
                    Name = "Morning",
                    StartTime = new TimeOnly(8, 0),
                    EndTime = new TimeOnly(16, 0),
                    GraceMinutes = 15,
                    LunchMinutes = 60
                },
                new Shift
                {
                    Name = "Afternoon",
                    StartTime = new TimeOnly(14, 0),
                    EndTime = new TimeOnly(22, 0),
                    GraceMinutes = 15,
                    LunchMinutes = 60
                },
                new Shift
                {
                    Name = "Night",
                    StartTime = new TimeOnly(22, 0),
                    EndTime = new TimeOnly(6, 0),
                    GraceMinutes = 15,
                    LunchMinutes = 60
                });
        }

        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
        if (adminUser == null)
        {
            context.Users.Add(new User
            {
                Username = "admin",
                PasswordHash = passwordHasher.Hash("Password123!"),
                FullName = "System Administrator",
                Email = "admin@example.com",
                Role = "Admin",
                IsActive = true
            });
        }
        else
        {
            adminUser.PasswordHash = passwordHasher.Hash("Password123!");
            context.Users.Update(adminUser);
        }

        await context.SaveChangesAsync();
    }
}
