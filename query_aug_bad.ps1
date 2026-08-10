$connectionString = "Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
$query = "SELECT COUNT(*) as Cnt FROM RawAttendanceLogs WHERE PunchTime >= '2026-08-01' AND CreatedAt < '2026-08-10 10:50:00'"
Invoke-Sqlcmd -ConnectionString $connectionString -Query $query | Format-Table -AutoSize
