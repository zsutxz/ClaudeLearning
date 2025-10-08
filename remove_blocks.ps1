$scenePath = '.\\UnityProject\\Assets\\Scenes\\MainMenu.unity'
$bakPath = $scenePath + '.bak2'
Copy-Item -Path $scenePath -Destination $bakPath -Force
$lines = Get-Content -LiteralPath $scenePath -Encoding UTF8
$indices = @()
for ($i=0; $i -lt $lines.Count; $i++) {
  if ($lines[$i].TrimStart().StartsWith('--- !u!114')) { $indices += $i }
}
$tot = $indices.Count
if ($tot -eq 0) { Write-Output 'NO_MONOBEHAVIOUR_BLOCKS_FOUND'; exit 0 }
$removeCount = [int]([math]::Floor($tot/2))
$removeRanges = @()
for ($k=0; $k -lt $removeCount; $k++) {
  $start = $indices[$k]
  if ($k + 1 -lt $indices.Count) { $end = $indices[$k+1] - 1 } else { $end = $lines.Count - 1 }
  $removeRanges += ,@($start,$end)
}
$new = New-Object System.Collections.Generic.List[string]
for ($i=0; $i -lt $lines.Count; $i++) {
  $skip = $false
  foreach ($r in $removeRanges) {
    if ($i -ge $r[0] -and $i -le $r[1]) { $skip = $true; break }
  }
  if (-not $skip) { $new.Add($lines[$i]) }
}
$new | Set-Content -LiteralPath $scenePath -Encoding UTF8
Write-Output "TOTAL_BLOCKS:$tot"
Write-Output "REMOVED_COUNT:$removeCount"
