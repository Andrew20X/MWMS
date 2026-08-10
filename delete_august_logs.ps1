$connectionString = "Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
$query = @"
DELETE FROM Attendances WHERE Date >= '2026-08-01'
DELETE FROM RawAttendanceLogs WHERE PunchTime >= '2026-08-01' OR CreatedAt < '2026-08-10 10:50:00' AND PunchTime >= '2026-08-01'
"@
Invoke-Sqlcmd -ConnectionString $connectionString -Query $query
