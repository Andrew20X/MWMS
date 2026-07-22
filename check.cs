using System;
using System.Data.SqlClient;

class Program {
    static void Main() {
        string connStr = "Server=.\\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;";
        using(var conn = new SqlConnection(connStr)) {
            conn.Open();
            using(var cmd = new SqlCommand("SELECT TOP 5 PunchTime, IsProcessed FROM RawAttendanceLogs WHERE EmployeeId = 335 ORDER BY PunchTime DESC", conn)) {
                using(var reader = cmd.ExecuteReader()) {
                    while(reader.Read()) {
                        Console.WriteLine(reader[0] + " - Processed: " + reader[1]);
                    }
                }
            }
        }
    }
}
