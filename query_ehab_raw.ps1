$connectionString = "Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
$query = "SELECT * FROM RawAttendanceLogs WHERE EmployeeId = (SELECT Id FROM Employees WHERE FirstName = 'Ehab' AND LastName LIKE '%Ali%') AND PunchTime >= '2026-08-10'"
Invoke-Sqlcmd -ConnectionString $connectionString -Query $query | Format-Table -AutoSize
