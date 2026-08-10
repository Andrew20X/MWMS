$connectionString = "Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
$query = @"
SELECT a.Date, a.CheckIn, a.CheckOut, a.Status, a.LateMinutes, a.EarlyLeaveMinutes, a.OvertimeMinutes, a.WorkedHours 
FROM Attendances a
JOIN Employees e ON a.EmployeeId = e.Id
WHERE e.EmployeeCode = '119'
ORDER BY a.Date
"@
Invoke-Sqlcmd -ConnectionString $connectionString -Query $query | Format-Table -AutoSize
