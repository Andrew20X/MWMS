$conn = New-Object System.Data.SqlClient.SqlConnection("Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;")
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT Id, FirstName, LastName, JobTitle FROM Employees"
$reader = $cmd.ExecuteReader()
$results = @()
while ($reader.Read()) {
    $results += [PSCustomObject]@{
        Id = $reader["Id"]
        FirstName = $reader["FirstName"]
        LastName = $reader["LastName"]
        JobTitle = $reader["JobTitle"]
    }
}
$conn.Close()
$results | ConvertTo-Json -Depth 10 | Out-File "d:\MWMS\db_employees.json" -Encoding utf8
