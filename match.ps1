$db = Get-Content "d:\MWMS\db_analysis.json" -Raw | ConvertFrom-Json
$screenshot = Get-Content "d:\MWMS\screenshot_data.json" -Raw | ConvertFrom-Json

$employees = $db.Employees
$positions = $db.Positions
$users = $db.Users

# Helper for fuzzy matching
function Get-BestMatch ($name) {
    $parts = $name -split ' '
    $bestMatch = $null
    $maxMatches = 0
    foreach ($emp in $employees) {
        $empFullName = "$($emp.FirstName) $($emp.LastName)"
        $empParts = $empFullName -split ' '
        
        $mCount = 0
        foreach ($p in $parts) {
            foreach ($ep in $empParts) {
                if ($p -ne '.' -and $ep -ne '.' -and $p.Length -gt 2 -and $ep.Length -gt 2) {
                    if ($p -match $ep -or $ep -match $p) {
                        $mCount++
                    }
                }
            }
        }
        if ($mCount -gt $maxMatches) {
            $maxMatches = $mCount
            $bestMatch = $emp
        }
    }
    return $bestMatch
}

$md = "# Employee Data Fix Plan`n`n"

foreach ($s in $screenshot) {
    $emp = Get-BestMatch $s.Name
    if ($emp -eq $null) {
        # Try finding by exact DeviceUserId
        if ($s.FingerprintId -ne $null) {
            $emp = $employees | Where-Object { $_.DeviceUserId -eq $s.FingerprintId } | Select-Object -First 1
        }
    }

    if ($emp -eq $null) {
        $md += "### NO MATCH FOUND for $($s.Name)`n"
        continue
    }

    $md += "### Match: $($s.Name)`n"
    $md += "- **DB Name**: $($emp.FirstName) $($emp.LastName) (ID: $($emp.Id))`n"
    
    # Check what needs to change
    $changes = @()
    if ($s.FingerprintId -ne $null -and $s.FingerprintId -ne $emp.DeviceUserId) {
        $changes += "Update Fingerprint ID: $($emp.DeviceUserId) -> $($s.FingerprintId)"
    }
    if ($s.Title -ne $null -and $s.Title -ne $emp.PositionName) {
        $changes += "Update Job Title: $($emp.PositionName) -> $($s.Title)"
    }
    if ($s.Email -ne $null -and $s.Email -ne $emp.Email) {
        $changes += "Update Email: $($emp.Email) -> $($s.Email)"
    }

    if ($changes.Count -gt 0) {
        foreach ($c in $changes) {
            $md += "- [x] $c`n"
        }
    } else {
        $md += "- No changes needed.`n"
    }
}

$md | Out-File "d:\MWMS\analysis_report.md" -Encoding utf8
