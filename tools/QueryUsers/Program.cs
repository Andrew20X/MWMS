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
            string sql = "DELETE sd FROM SalaryDeductions sd INNER JOIN Employees e ON sd.EmployeeId = e.Id WHERE e.DeviceUserId <= 0 AND sd.Reason LIKE 'AWOL%'";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine($"Deleted {rowsAffected} invalid AWOL deductions.");
            }
        }
    }
}
