$connectionString = "Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
$query = "SELECT COUNT(*) as Cnt FROM RawAttendanceLogs WHERE PunchTime >= '2026-07-01' AND PunchTime < '2026-08-01'"
Invoke-Sqlcmd -ConnectionString $connectionString -Query $query
