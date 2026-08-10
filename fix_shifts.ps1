$connectionString = "Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;"

$query = @"
UPDATE Shifts 
SET StartTime = '09:00:00', EndTime = '17:00:00' 
WHERE Name = 'Morning' OR Name = 'Day Shift';

UPDATE RawAttendanceLogs 
SET IsProcessed = 0;

UPDATE Attendances
SET LateMinutes = 0,
    EarlyLeaveMinutes = 0,
    OvertimeMinutes = 0,
    WorkedHours = 0;
"@

Invoke-Sqlcmd -ConnectionString $connectionString -Query $query
Write-Host "Database updated."
