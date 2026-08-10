$connectionString = "Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
$query = "SELECT Id, EmployeeCode, DeviceUserId, FirstName, LastName FROM Employees WHERE FirstName LIKE '%Ziad%'"
Invoke-Sqlcmd -ConnectionString $connectionString -Query $query | Format-Table -AutoSize
