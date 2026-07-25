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
            string sql = "DELETE FROM Users WHERE Id = 223";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                int deleted = command.ExecuteNonQuery();
                Console.WriteLine($"Duplicate user deleted: {deleted}");
            }
        }
    }
}
