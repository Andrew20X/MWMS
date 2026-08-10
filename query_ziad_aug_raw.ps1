$connectionString = "Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
$query = "SELECT * FROM RawAttendanceLogs WHERE EmployeeId = 281 AND PunchTime >= '2026-08-01'"
Invoke-Sqlcmd -ConnectionString $connectionString -Query $query | Format-Table -AutoSize
