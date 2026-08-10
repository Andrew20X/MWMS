$connectionString = "Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
$query = "SELECT a.Date, a.CheckIn, a.CheckOut, e.FirstName, e.LastName FROM Attendances a JOIN Employees e ON a.EmployeeId = e.Id WHERE a.Date = '2026-08-10' AND e.FirstName LIKE '%Al-Hassan%'"
Invoke-Sqlcmd -ConnectionString $connectionString -Query $query | Format-Table -AutoSize
