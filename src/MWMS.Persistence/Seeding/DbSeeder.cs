using Microsoft.EntityFrameworkCore;
using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;
using MWMS.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(17, 0),
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

        await SeedEmployeesAsync(context, passwordHasher);
    }

    private static async Task SeedEmployeesAsync(AppDbContext context, IPasswordHasher passwordHasher)
    {
        var empData = new List<(string Name, string Title, string Manager)> {
            ("Hossam Sherif", "Managing Director", null),
            ("Fatma Tarek Mohamed Mahmoud", "Legal Officer", "Hossam Sherif"),
            ("Reham Mohamed Abd El-Razek Mostafa", "Office Manager", "Hossam Sherif"),
            ("Abdelaziz Faiz Ismail Abaza", "Sales Manager", "Hossam Sherif"),
            ("Sales Team", "Employee", "Sales Manager"),
            ("Abdel Aziz Abaza", "BD Manager", "Hossam Sherif"),
            ("Couriers", "Employee", "Abdel Aziz Abaza"),
            ("Ahmed Abdel Moneim Abdel Hamid Ghazi Sharif", "Quality Control Manager", "Hossam Sherif"),
            ("Quality Assistant", "Employee", "Ahmed Abdel Moneim Abdel Hamid Ghazi Sharif"),
            ("Karim Mohamed Hanafi Wahba", "HR Manager", "Hossam Sherif"),
            ("Sohaila Hany Mohammed Abd El-Aziz", "HR Specialist", "Karim Mohamed Hanafi Wahba"),
            ("Service Team", "Employee", "Sohaila Hany Mohammed Abd El-Aziz"),
            ("Security", "Employee", "Sohaila Hany Mohammed Abd El-Aziz"),
            ("Mohamed Nasr Hassan Ahmed", "Operations Manager", "Hossam Sherif"),
            ("Esraa Mohamed Abdallah Mostafa El-Assal", "HR & Administration Coordinator", "Mohamed Nasr Hassan Ahmed"),
            ("Al-Hassan Mostafa Mohammad Mostafa", "Administrative Assistant Manager", "Mohamed Nasr Hassan Ahmed"),
            ("Wael Abouzeid Youssef Abouzeid", "Driver", "Al-Hassan Mostafa Mohammad Mostafa"),
            
            ("Gaber Ammar", "Technical Director", null),
            ("Rodina Ahmed", "CEO Assistant", "Gaber Ammar"),
            ("Amr Mabrouk Mohamed Ahmed Abdo", "Finance Manager", "Gaber Ammar"),
            ("Abanoub Samir Agaby Issa", "Financial Accountant", "Amr Mabrouk Mohamed Ahmed Abdo"),
            ("Donia Maher Mohamed Gamal El Din Ahmed", "Accounts Receivable Accountant", "Abanoub Samir Agaby Issa"),
            ("Donia Mohammed Ali Al-Sayed Al-Shamy", "Senior Supplier Accountant", "Donia Maher Mohamed Gamal El Din Ahmed"),
            ("Mahmoud Mehny", "Courier", "Donia Mohammed Ali Al-Sayed Al-Shamy"),
            ("Accountant", "Accountant", "Amr Mabrouk Mohamed Ahmed Abdo"),
            ("Senior Treasury", "Employee", "Amr Mabrouk Mohamed Ahmed Abdo"),
            ("Ramy Zakaria Mohamed Zakaria Gamil", "Treasury officer", "Senior Treasury"),
            ("Ehab Ali Abdel Majeed", "Purchasing Supervisor", "Gaber Ammar"),
            ("Nesma Mahmoud Abdel Aziz El-Sayed Nassar", "Procurement assistant", "Ehab Ali Abdel Majeed"),
            ("Buyers Team", "Employee", "Nesma Mahmoud Abdel Aziz El-Sayed Nassar"),
            ("Mona Gabr Ali Mohammad Ibrahim", "Logistic Specialist", "Ehab Ali Abdel Majeed"),
            ("Technical Manager", "Technical Manager", "Gaber Ammar"),
            ("Technical Supervisor", "Employee", "Technical Manager"),
            ("Instrumentation and Control Engineers", "Employee", "Technical Supervisor"),
            ("Ahmed Hany Mohammed Abdelkhaleq Al-Timami", "R&D Engineer", "Technical Manager"),
            ("Ahmed Khalifa", "Inventory Supervisor", "Technical Manager"),
            ("Hesham Ahmed Abdel Raouf Mahmoud", "Warehouse Manager assistant", "Ahmed Khalifa"),
            ("Ahmed Khater", "Mechanical Workshop Manager", "Gaber Ammar"),
            ("Workshop Team", "Employee", "Ahmed Khater"),
            ("Sherif Salah Mohammed Abdel-Jalil", "Project Manager", "Gaber Ammar"),
            ("Kyrollos Nabil", "Project Manager", "Sherif Salah Mohammed Abdel-Jalil"),
            ("Mohamed Hatem", "Civil Engineer", "Kyrollos Nabil"),
            ("Mohamed El Desouky El Desouky El-Saeidy", "Mechanical Engineer Technical Office", "Kyrollos Nabil"),
            ("Ahmed Ibrahim Al-Dessouki Sayed Qasim Mohammad", "Document Controler", "Kyrollos Nabil"),
            ("Loay Reda Ismail Al-Aswad", "Occupational Health and Safety Engineer", "Gaber Ammar")
        };

        if (await context.Employees.AnyAsync()) return;

        var defaultDept = await context.Departments.FirstAsync();
        var defaultPos = await context.Positions.FirstAsync();
        var defaultShift = await context.Shifts.FirstAsync();

        var createdEmployees = new Dictionary<string, Employee>();
        var createdUsers = new List<User>();

        int code = 1000;
        foreach (var data in empData)
        {
            var parts = data.Name.Split(" ");
            var firstName = parts[0];
            var lastName = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : "";
            
            var email = "(No Email)";

            var emp = new Employee
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                EmployeeCode = "EMP" + (code++).ToString(),
                DepartmentId = defaultDept.Id,
                PositionId = defaultPos.Id,
                ShiftId = defaultShift.Id
            };
            context.Employees.Add(emp);
            createdEmployees[data.Name] = emp;
        }

        await context.SaveChangesAsync();

        foreach (var data in empData)
        {
            var emp = createdEmployees[data.Name];
            if (!string.IsNullOrEmpty(data.Manager) && createdEmployees.ContainsKey(data.Manager))
            {
                emp.ManagerId = createdEmployees[data.Manager].Id;
            }

            var parts = data.Name.Split(" ");
            var username = parts[0].ToLower() + (parts.Length > 1 ? parts.Last().ToLower() : "");
            
            // Check for duplicate username
            if (createdUsers.Any(u => u.Username == username)) {
                username += code.ToString(); // append something to make it unique if needed
            }

            var user = new User
            {
                Username = username,
                PasswordHash = passwordHasher.Hash("Password123!"),
                FullName = data.Name,
                Email = emp.Email,
                Role = "Employee",
                IsActive = true
            };
            context.Users.Add(user);
            createdUsers.Add(user);
        }
        
        await context.SaveChangesAsync();
    }
}
