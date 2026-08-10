$connectionString = "Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
$query = "SELECT TOP 10 a.Date, a.CheckIn, e.FirstName, e.LastName FROM Attendances a JOIN Employees e ON a.EmployeeId = e.Id WHERE a.Date = '2026-08-10' ORDER BY a.CheckIn DESC"
Invoke-Sqlcmd -ConnectionString $connectionString -Query $query | Format-Table -AutoSize
