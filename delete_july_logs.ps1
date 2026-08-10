$connectionString = "Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
$query = @"
-- Delete July 2026 Attendances (processed)
DELETE FROM Attendances WHERE Date >= '2026-07-01' AND Date < '2026-08-01'

-- Delete July 2026 Raw Logs
DELETE FROM RawAttendanceLogs WHERE PunchTime >= '2026-07-01' AND PunchTime < '2026-08-01'
"@
Invoke-Sqlcmd -ConnectionString $connectionString -Query $query
