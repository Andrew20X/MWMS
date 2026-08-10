$db = Get-Content "d:\MWMS\db_analysis.json" -Raw | ConvertFrom-Json
$screenshot = Get-Content "d:\MWMS\screenshot_data.json" -Raw | ConvertFrom-Json

$employees = $db.Employees
$positions = $db.Positions
$users = $db.Users

function Normalize-Name ($name) {
    if ([string]::IsNullOrWhiteSpace($name)) { return "" }
    $name = $name -replace '-', ' '
    $name = $name -replace '\s+', ' '
    return $name.ToLower().Trim()
}

$md = "# Proposed Implementation Plan`n`n"
$md += "This plan maps the screenshot data to the existing employees in the database. I will update the employees and the initial seed data.`n`n"

$sqlUpdates = ""

foreach ($s in $screenshot) {
    $emp = $null
    
    # 1. Match by Fingerprint ID if available and valid
    if ($s.FingerprintId -ne $null) {
        $emp = $employees | Where-Object { $_.DeviceUserId -eq $s.FingerprintId } | Select-Object -First 1
    }

    # 2. Match by exact normalized name
    if ($emp -eq $null) {
        $sNorm = Normalize-Name $s.Name
        $emp = $employees | Where-Object { 
            $empNorm = Normalize-Name "$($_.FirstName) $($_.LastName)"
            $empNorm -eq $sNorm
        } | Select-Object -First 1
    }

    # 3. Match by fuzzy name
    if ($emp -eq $null) {
        $sNorm = Normalize-Name $s.Name
        $parts = $sNorm -split ' '
        $bestMatch = $null
        $maxMatches = 0
        foreach ($e in $employees) {
            $eNorm = Normalize-Name "$($e.FirstName) $($e.LastName)"
            $eParts = $eNorm -split ' '
            $mCount = 0
            foreach ($p in $parts) {
                if ($eParts -contains $p) {
                    $mCount++
                }
            }
            if ($mCount -gt $maxMatches) {
                $maxMatches = $mCount
                $bestMatch = $e
            }
        }
        if ($maxMatches -ge 2) {
            $emp = $bestMatch
        }
    }

    if ($emp -eq $null) {
        $md += "### ⚠️ NO MATCH FOUND: $($s.Name) (Fingerprint: $($s.FingerprintId))`n"
        continue
    }

    $md += "### Match: $($s.Name)`n"
    $md += "- **Matched DB Employee**: $($emp.FirstName) $($emp.LastName) (ID: $($emp.Id), DeviceUserId: $($emp.DeviceUserId))`n"
    
    $changes = @()
    $sql = ""
    
    # Check what needs to change
    if ($s.FingerprintId -ne $null -and $s.FingerprintId -ne $emp.DeviceUserId) {
        $changes += "Update Fingerprint ID: $($emp.DeviceUserId) -> $($s.FingerprintId)"
        $sql += "DeviceUserId = $($s.FingerprintId), "
    }
    if ($s.Title -ne $null -and $s.Title -ne $emp.PositionName) {
        $changes += "Update Job Title: $($emp.PositionName) -> $($s.Title)"
    }
    if ($s.Email -ne $null -and $s.Email -ne $emp.Email) {
        $changes += "Update Email: $($emp.Email) -> $($s.Email)"
        $sql += "Email = '$($s.Email)', "
    }
    if ($s.Name -ne "$($emp.FirstName) $($emp.LastName)") {
        $changes += "Update Name: $($emp.FirstName) $($emp.LastName) -> $($s.Name)"
        $parts = $s.Name -split ' '
        $fn = $parts[0]
        $ln = ""
        if ($parts.Length -gt 1) {
            $ln = $parts[1..($parts.Length-1)] -join ' '
        }
        $sql += "FirstName = '$fn', LastName = '$ln', "
    }

    if ($changes.Count -gt 0) {
        foreach ($c in $changes) {
            $md += "- [x] $c`n"
        }
    } else {
        $md += "- No changes needed.`n"
    }
}

$md | Out-File "d:\MWMS\plan_draft.md" -Encoding utf8
