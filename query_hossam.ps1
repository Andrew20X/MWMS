$connectionString = "Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
$query = "SELECT Id, EmployeeCode, FirstName, LastName FROM Employees WHERE FirstName LIKE '%Hossam%' AND LastName LIKE '%Sherif%'"
Invoke-Sqlcmd -ConnectionString $connectionString -Query $query | Format-List
