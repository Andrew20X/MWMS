$connectionString = "Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
$query = @"
SELECT FORMAT(PunchTime, 'yyyy-MM') as Month, COUNT(*) as Count 
FROM RawAttendanceLogs 
GROUP BY FORMAT(PunchTime, 'yyyy-MM') 
ORDER BY Month
"@
Invoke-Sqlcmd -ConnectionString $connectionString -Query $query | Format-Table -AutoSize
