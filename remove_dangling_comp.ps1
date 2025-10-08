$scenePath = '.\\UnityProject\\Assets\\Scenes\\MainMenu.unity'
$bak = $scenePath + '.pre_remove_comp.bak'
Copy-Item -Path $scenePath -Destination $bak -Force
$lines = Get-Content -LiteralPath $scenePath -Encoding UTF8
$matches = @()
for ($i=0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match 'fileID:\s*1000000052\b') { $matches += $i }
}
if ($matches.Count -eq 0) { Write-Output 'NO_MATCHES_FOUND'; exit 0 }
$toRemoveRanges = @()
foreach ($m in $matches) {
    # find header start by searching backwards for line starting with '--- !u!'
    $start = $m
    while ($start -ge 0 -and -not ($lines[$start].TrimStart().StartsWith('--- !u!'))) { $start-- }
    if ($start -lt 0) { $start = 0 }
    # find next header after m
    $end = $m
    $n = $m+1
    while ($n -lt $lines.Count -and -not ($lines[$n].TrimStart().StartsWith('--- !u!'))) { $n++ }
    $end = if ($n -lt $lines.Count) { $n - 1 } else { $lines.Count - 1 }
    $toRemoveRanges += ,@($start,$end)
}
# Merge overlapping ranges
$toRemoveRanges = $toRemoveRanges | Sort-Object { $_[0] }
$merged = @()
foreach ($r in $toRemoveRanges) {
    if ($merged.Count -eq 0) { $merged += $r; continue }
    $last = $merged[$merged.Count-1]
    if ($r[0] -le $last[1]+1) { $merged[$merged.Count-1] = @($last[0], [Math]::Max($last[1], $r[1])) } else { $merged += $r }
}
# Build new content skipping merged ranges
$new = New-Object System.Collections.Generic.List[string]
for ($i=0; $i -lt $lines.Count; $i++) {
    $skip = $false
    foreach ($r in $merged) { if ($i -ge $r[0] -and $i -le $r[1]) { $skip = $true; break } }
    if (-not $skip) { $new.Add($lines[$i]) }
}
# Write back
$new | Set-Content -LiteralPath $scenePath -Encoding UTF8
Write-Output ('BACKUP:' + $bak)
Write-Output ('REMOVED_RANGES_COUNT:' + $merged.Count)
foreach ($r in $merged) { Write-Output ('REMOVED_RANGE:' + $r[0] + '-' + $r[1]) }
