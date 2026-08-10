using Microsoft.EntityFrameworkCore;
using MWMS.Domain.Entities;
using MWMS.Persistence.Context;

namespace MWMS.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (!await context.Departments.AnyAsync())
        {
            context.Departments.AddRange(
                new Department { Name = "General", Description = "Default department" },
                new Department { Name = "Operations", Description = "Operations department" });
        }

        if (!await context.Positions.AnyAsync())
        {
            context.Positions.AddRange(
                new Position { Name = "Staff" },
                new Position { Name = "Supervisor" });
        }

        if (!await context.Shifts.AnyAsync())
        {
            context.Shifts.AddRange(
                new Shift
                {
                    Name = "Morning",
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(17, 0),
                    GraceMinutes = 15,
                    LunchMinutes = 60
                },
                new Shift
                {
                    Name = "Night Shift",
                    StartTime = new TimeOnly(22, 0),
                    EndTime = new TimeOnly(6, 0),
                    GraceMinutes = 15,
                    LunchMinutes = 30
                });
        }

        await context.SaveChangesAsync();
    }
}
