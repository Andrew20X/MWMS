$dir = "d:\MWMS"
Get-ChildItem -Path $dir -Recurse -Include *.tsx, *.ts, *.css, *.cs, *.html, *.md | ForEach-Object {
    $content = [System.IO.File]::ReadAllText($_.FullName)
    $newContent = [System.Text.RegularExpressions.Regex]::Replace($content, "fontWeight:\s*'bold'", "fontWeight: 'normal'")
    $newContent = [System.Text.RegularExpressions.Regex]::Replace($newContent, 'fontWeight:\s*"bold"', 'fontWeight: "normal"')
    $newContent = [System.Text.RegularExpressions.Regex]::Replace($newContent, "fontWeight:\s*[56789]00", "fontWeight: 400")
    $newContent = [System.Text.RegularExpressions.Regex]::Replace($newContent, "font-weight:\s*bold", "font-weight: normal")
    $newContent = [System.Text.RegularExpressions.Regex]::Replace($newContent, "font-weight:\s*[56789]00", "font-weight: 400")
    $newContent = [System.Text.RegularExpressions.Regex]::Replace($newContent, "<strong\b(.*?)>", "<span$1>")
    $newContent = [System.Text.RegularExpressions.Regex]::Replace($newContent, "</strong>", "</span>")
    $newContent = [System.Text.RegularExpressions.Regex]::Replace($newContent, "<b\b(.*?)>", "<span$1>")
    $newContent = [System.Text.RegularExpressions.Regex]::Replace($newContent, "</b>", "</span>")
    if ($content -ne $newContent) {
        [System.IO.File]::WriteAllText($_.FullName, $newContent, (New-Object System.Text.UTF8Encoding($false)))
        Write-Host "Updated $($_.FullName)"
    }
}
