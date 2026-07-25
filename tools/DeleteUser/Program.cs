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
            string sql = "DELETE FROM Users WHERE Email = 'Andrewraafat2016@yahoo.com' AND Username = 'EMP-1111'";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine($"Rows deleted: {rowsAffected}");
            }
        }
    }
}
