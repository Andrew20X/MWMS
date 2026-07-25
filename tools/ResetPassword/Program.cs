using System;
using System.Data;
using Microsoft.Data.SqlClient;
using BCrypt.Net;

namespace ResetPassword
{
    class Program
    {
        static void Main(string[] args)
        {
            string newPassword = "Password123!";
            string hash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            
            string connectionString = "Server=.\\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;";
            
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "UPDATE Users SET PasswordHash = @hash WHERE Role = 'Admin'";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@hash", hash);
                        int rowsAffected = command.ExecuteNonQuery();
                        Console.WriteLine($"Successfully updated password for {rowsAffected} admin users.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating passwords: {ex.Message}");
                }
            }
        }
    }
}
