$connectionString = "Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
$query = "SELECT TOP 5 Date, CheckIn, CheckOut FROM Attendances WHERE EmployeeId = 281 AND Date >= '2026-07-01' AND Date < '2026-08-01' ORDER BY Date"
Invoke-Sqlcmd -ConnectionString $connectionString -Query $query | Format-Table -AutoSize
