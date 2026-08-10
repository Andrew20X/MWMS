$connectionString = "Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
$query = @"
DELETE FROM SalaryDeductions WHERE RelatedAttendanceId IN (SELECT Id FROM Attendances WHERE Date >= '2026-08-01')
DELETE FROM Attendances WHERE Date >= '2026-08-01'
DELETE FROM RawAttendanceLogs WHERE PunchTime >= '2026-08-01'
"@
Invoke-Sqlcmd -ConnectionString $connectionString -Query $query
