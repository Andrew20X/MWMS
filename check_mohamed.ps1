$connectionString = "Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
$query = @"
SELECT Id, EmployeeCode, FirstName, LastName, DeviceUserId 
FROM Employees 
WHERE FirstName LIKE '%Mohamed%' OR LastName LIKE '%Desouky%'
"@
Invoke-Sqlcmd -ConnectionString $connectionString -Query $query | Format-Table -AutoSize
