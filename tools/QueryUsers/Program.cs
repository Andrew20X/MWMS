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
            string[] tables = { "LeaveRequests", "OvertimeRequests", "CorrectionRequests", "SalaryDeductions" };
            
            foreach (var table in tables)
            {
                int count = 0;
                string sql = $"SELECT Id, Reason FROM {table}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        var updates = new System.Collections.Generic.List<Tuple<int, string>>();
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string reason = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                            
                            int index = reason.IndexOf("* Approval Status:");
                            if (index >= 0)
                            {
                                string newReason = reason.Substring(0, index).TrimEnd();
                                updates.Add(Tuple.Create(id, newReason));
                            }
                        }
                        reader.Close();
                        
                        foreach (var update in updates)
                        {
                            string updateSql = $"UPDATE {table} SET Reason = @r WHERE Id = @i";
                            using (SqlCommand updateCmd = new SqlCommand(updateSql, connection))
                            {
                                updateCmd.Parameters.AddWithValue("@r", update.Item2);
                                updateCmd.Parameters.AddWithValue("@i", update.Item1);
                                updateCmd.ExecuteNonQuery();
                                count++;
                            }
                        }
                    }
                }
                Console.WriteLine($"Updated {count} rows in {table}");
            }
        }
    }
}
