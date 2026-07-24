using Microsoft.EntityFrameworkCore;
using MWMS.Domain.Entities;

namespace MWMS.Persistence.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<CorrectionRequest> CorrectionRequests => Set<CorrectionRequest>();
    public DbSet<OvertimeRequest> OvertimeRequests => Set<OvertimeRequest>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<RawAttendanceLog> RawAttendanceLogs => Set<RawAttendanceLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Feature 2: Approval audit trail
    public DbSet<ApprovalHistory> ApprovalHistories => Set<ApprovalHistory>();

    // Feature 4: Leave balance management
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    
    // Feature: Salary Deduction
    public DbSet<SalaryDeduction> SalaryDeductions => Set<SalaryDeduction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Self-referencing FK: Employee.ManagerId → Employee.Id
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Manager)
            .WithMany(e => e.Subordinates)
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        modelBuilder.Entity<SalaryDeduction>()
            .HasOne(sd => sd.Employee)
            .WithMany()
            .HasForeignKey(sd => sd.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SalaryDeduction>()
            .Property(sd => sd.DeductionAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<SalaryDeduction>()
            .HasOne(sd => sd.RelatedAttendance)
            .WithMany()
            .HasForeignKey(sd => sd.RelatedAttendanceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}