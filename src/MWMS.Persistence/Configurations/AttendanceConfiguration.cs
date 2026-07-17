using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MWMS.Domain.Entities;

namespace MWMS.Persistence.Configurations;

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Date)
               .IsRequired();

        builder.Property(a => a.Status)
               .HasConversion<int>();

        builder.HasOne(a => a.Employee)
               .WithMany(e => e.Attendances)
               .HasForeignKey(a => a.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}