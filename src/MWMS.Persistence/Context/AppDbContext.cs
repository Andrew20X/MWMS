using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MWMS.Domain.Entities;

namespace MWMS.Persistence.Context;

public class AppDbContext : DbContext
{
    private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor? _httpContextAccessor;

    public AppDbContext(DbContextOptions<AppDbContext> options, Microsoft.AspNetCore.Http.IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
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
    public DbSet<SubmissionComment> SubmissionComments => Set<SubmissionComment>();

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

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = await base.SaveChangesAsync(cancellationToken);
        if (auditEntries != null && auditEntries.Count > 0)
        {
            OnAfterSaveChanges(auditEntries);
            await base.SaveChangesAsync(cancellationToken);
        }
        return result;
    }

    public override int SaveChanges()
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = base.SaveChanges();
        if (auditEntries != null && auditEntries.Count > 0)
        {
            OnAfterSaveChanges(auditEntries);
            base.SaveChanges();
        }
        return result;
    }

    private class AuditEntryHelper
    {
        public EntityEntry Entry { get; }
        public AuditLog AuditLog { get; }
        public AuditEntryHelper(EntityEntry entry, AuditLog auditLog)
        {
            Entry = entry;
            AuditLog = auditLog;
        }
    }

        private List<AuditEntryHelper> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        
        var auditEntries = new List<AuditEntryHelper>();
        var userIdStr = _httpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int? userId = null;
        if (int.TryParse(userIdStr, out int uid)) userId = uid;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.Entity is RawAttendanceLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditLog
            {
                EntityName = entry.Entity.GetType().Name,
                ActionType = entry.State.ToString(),
                AdminUserId = userId,
                Timestamp = DateTime.UtcNow
            };

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary)
                    continue;

                string propertyName = property.Metadata.Name;

                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues[propertyName] = property.CurrentValue;
                        break;
                    case EntityState.Deleted:
                        oldValues[propertyName] = property.OriginalValue;
                        break;
                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            oldValues[propertyName] = property.OriginalValue;
                            newValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }

            auditEntry.OldValues = oldValues.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(oldValues) : string.Empty;
            auditEntry.NewValues = newValues.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(newValues) : string.Empty;
            
            auditEntries.Add(new AuditEntryHelper(entry, auditEntry));
        }

        return auditEntries;
    }

    private void OnAfterSaveChanges(List<AuditEntryHelper> auditEntries)
    {
        foreach (var helper in auditEntries)
        {
            var pk = helper.Entry.Metadata.FindPrimaryKey()?.Properties.FirstOrDefault();
            var pkName = pk?.Name;
            
            string entityId = "Unknown";
            object? idValue = null;

            if (pkName != null)
            {
                // Reading directly from the C# entity object bypasses any EntityEntry caching
                var entityObj = helper.Entry.Entity;
                idValue = entityObj.GetType().GetProperty(pkName)?.GetValue(entityObj);
                entityId = idValue?.ToString() ?? "Unknown";
            }

            helper.AuditLog.EntityId = entityId;
            
            // If the entity was added, the new values didn't have the generated ID.
            if (helper.AuditLog.ActionType == "Added" && helper.AuditLog.NewValues != string.Empty)
            {
                try
                {
                    var newVals = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(helper.AuditLog.NewValues) ?? new Dictionary<string, object?>();
                    if (pkName != null)
                    {
                        newVals[pkName] = idValue;
                        helper.AuditLog.NewValues = System.Text.Json.JsonSerializer.Serialize(newVals);
                    }
                }
                catch { }
            }

            AuditLogs.Add(helper.AuditLog);
        }
    }
}