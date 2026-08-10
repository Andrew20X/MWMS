$connectionString = "Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
$query = @"
SELECT TOP 5 PunchTime, CreatedAt 
FROM RawAttendanceLogs 
WHERE PunchTime >= '2026-08-01'
"@
Invoke-Sqlcmd -ConnectionString $connectionString -Query $query | Format-Table -AutoSize
