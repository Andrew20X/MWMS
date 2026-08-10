$conn = New-Object System.Data.SqlClient.SqlConnection("Server=.\SQLEXPRESS;Database=MWMS_DB;Trusted_Connection=True;TrustServerCertificate=True;")
$conn.Open()

$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT e.DeviceUserId, e.FirstName, e.LastName, p.Name as PositionName FROM Employees e LEFT JOIN Positions p ON e.PositionId = p.Id"
$reader = $cmd.ExecuteReader()
$results = @{}
while ($reader.Read()) {
    $results[$reader["DeviceUserId"].ToString()] = @{
        FirstName = $reader["FirstName"]
        LastName = $reader["LastName"]
        PositionName = $reader["PositionName"]
    }
}
$conn.Close()

# Read fingerprint_list.txt
$lines = Get-Content "d:\MWMS\fingerprint_list.txt"
$md = @("| ID | Fingerprint Name | DB Name | Position |", "|---|---|---|---|")

foreach ($line in $lines) {
    if ($line -match 'ID:\s*(\d+)\s*\|\s*First:\s*(.*?)\s*\|') {
        $id = $matches[1]
        $fpName = $matches[2]
        
        $dbName = "-"
        $position = "-"
        if ($results.ContainsKey($id)) {
            $dbName = "$($results[$id].FirstName) $($results[$id].LastName)"
            $position = if ([string]::IsNullOrEmpty($results[$id].PositionName)) { "None" } else { $results[$id].PositionName }
        }
        $md += "| $id | $fpName | $dbName | $position |"
    }
}

$md | Out-File "d:\MWMS\fingerprint_positions.md" -Encoding utf8
