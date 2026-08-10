$conn = New-Object System.Data.SqlClient.SqlConnection("Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;")
$conn.Open()

$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT e.FirstName, e.LastName, p.Name as PositionName FROM Employees e LEFT JOIN Positions p ON e.PositionId = p.Id"
$reader = $cmd.ExecuteReader()
$results = @()
while ($reader.Read()) {
    $results += [PSCustomObject]@{
        FirstName = $reader["FirstName"]
        LastName = $reader["LastName"]
        PositionName = $reader["PositionName"]
    }
}
$conn.Close()
$results | ConvertTo-Json -Depth 10 | Out-File "d:\MWMS\employees_positions.json" -Encoding utf8
