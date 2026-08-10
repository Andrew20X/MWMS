$conn = New-Object System.Data.SqlClient.SqlConnection("Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;")
$conn.Open()

# Get Employees
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT e.Id, e.DeviceUserId, e.FirstName, e.LastName, e.Email, e.PositionId, p.Name as PositionName FROM Employees e LEFT JOIN Positions p ON e.PositionId = p.Id WHERE e.IsDeleted = 0"
$reader = $cmd.ExecuteReader()
$employees = @()
while ($reader.Read()) {
    $employees += [PSCustomObject]@{
        Id = $reader["Id"]
        DeviceUserId = $reader["DeviceUserId"]
        FirstName = $reader["FirstName"]
        LastName = $reader["LastName"]
        Email = $reader["Email"]
        PositionId = $reader["PositionId"]
        PositionName = $reader["PositionName"]
    }
}
$reader.Close()

# Get Users
$cmd.CommandText = "SELECT Id, FullName, Username, Email FROM Users WHERE IsDeleted = 0"
$reader = $cmd.ExecuteReader()
$users = @()
while ($reader.Read()) {
    $users += [PSCustomObject]@{
        Id = $reader["Id"]
        FullName = $reader["FullName"]
        Username = $reader["Username"]
        Email = $reader["Email"]
    }
}
$reader.Close()

# Get all positions
$cmd.CommandText = "SELECT Id, Name FROM Positions"
$reader = $cmd.ExecuteReader()
$positions = @()
while ($reader.Read()) {
    $positions += [PSCustomObject]@{
        Id = $reader["Id"]
        Name = $reader["Name"]
    }
}
$reader.Close()

$conn.Close()

@{
    Employees = $employees
    Users = $users
    Positions = $positions
} | ConvertTo-Json -Depth 10 | Out-File "d:\MWMS\db_analysis.json" -Encoding utf8
