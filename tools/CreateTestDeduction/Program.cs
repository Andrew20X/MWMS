using System;
using Microsoft.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connectionString = "Server=.\\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;";
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            int empId = 336;
            
            // Insert dummy attendance
            string sqlAtt = @"
                INSERT INTO Attendances (EmployeeId, Date, Status, IsUnexcused, AbsenceResolutionStatus, DeadlineForLeaveRequest, CreatedAt, UpdatedAt, IsDeleted, WorkedHours, LateMinutes, EarlyLeaveMinutes, OvertimeMinutes)
                OUTPUT INSERTED.Id
                VALUES (@empId, @date, 0, 1, 0, @deadline, GETUTCDATE(), GETUTCDATE(), 0, 0, 0, 0, 0)";
                
            int attendanceId = 0;
            using (SqlCommand command = new SqlCommand(sqlAtt, connection))
            {
                command.Parameters.AddWithValue("@empId", empId);
                command.Parameters.AddWithValue("@date", DateTime.Today.AddDays(-4)); // 4 days ago
                command.Parameters.AddWithValue("@deadline", DateTime.Today.AddDays(-1));
                attendanceId = (int)command.ExecuteScalar();
            }
            
            // Insert deduction
            string sqlDed = @"
                INSERT INTO SalaryDeductions (EmployeeId, RelatedAttendanceId, DeductionAmount, Reason, AppliedOnDate, Status, CreatedAt, UpdatedAt, IsDeleted)
                VALUES (@empId, @attId, 1.0, 'Test deduction for Ziad', GETUTCDATE(), 0, GETUTCDATE(), GETUTCDATE(), 0)";
                
            using (SqlCommand command = new SqlCommand(sqlDed, connection))
            {
                command.Parameters.AddWithValue("@empId", empId);
                command.Parameters.AddWithValue("@attId", attendanceId);
                command.ExecuteNonQuery();
            }
            
            Console.WriteLine("Test deduction created successfully!");
        }
    }
}
