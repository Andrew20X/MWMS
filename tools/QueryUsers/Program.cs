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
            string sql = "SELECT Id, EmployeeCode, FirstName, LastName, Email FROM Employees";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"Id: {reader["Id"]}, Code: {reader["EmployeeCode"]}, Name: {reader["FirstName"]} {reader["LastName"]}, Email: {reader["Email"]}");
                    }
                }
            }
        }
    }
}
