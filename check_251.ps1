$connectionString = "Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
$query = @"
SELECT Date, CheckIn, CheckOut, Status, LateMinutes, EarlyLeaveMinutes, OvertimeMinutes, WorkedHours 
FROM Attendances 
WHERE EmployeeId = 251
ORDER BY Date
"@
Invoke-Sqlcmd -ConnectionString $connectionString -Query $query | Format-Table -AutoSize
