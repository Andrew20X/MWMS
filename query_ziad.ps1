$connectionString = "Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
$query = "SELECT Id, EmployeeCode, DeviceUserId, FirstName, LastName FROM Employees WHERE FirstName LIKE '%Ziad%' OR LastName LIKE '%Ziad%' OR Id = 131"
Invoke-Sqlcmd -ConnectionString $connectionString -Query $query | Format-List
