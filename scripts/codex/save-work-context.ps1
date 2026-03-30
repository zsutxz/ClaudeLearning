param(
    [string]$ProjectRoot = (Get-Location).Path,
    [string]$TaskSummary = "Resume the most recent Codex task in this project.",
    [string]$Status = "in progress",
    [string]$OutputDir
)

$ErrorActionPreference = "Stop"

function Get-GitOutput {
    param(
        [string[]]$GitArgs
    )

    try {
        $output = & git -C $ProjectRoot @GitArgs 2>$null
        if ($LASTEXITCODE -eq 0) {
            return @($output)
        }
    } catch {
    }

    return @()
}

if (-not (Test-Path $ProjectRoot)) {
    throw "Project root does not exist: $ProjectRoot"
}

$resolvedRoot = (Resolve-Path $ProjectRoot).Path

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $resolvedRoot ".codex\handoffs"
}

New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

$timestamp = Get-Date
$filename = "{0}-current-work-context.md" -f $timestamp.ToString("yyyy-MM-dd-HHmm")
$outputPath = Join-Path $OutputDir $filename

$branch = (Get-GitOutput @("branch", "--show-current") | Select-Object -First 1)
if ([string]::IsNullOrWhiteSpace($branch)) {
    $branch = "unknown"
}

$gitStatus = Get-GitOutput @("status", "--short")
$changedFiles = @()

foreach ($line in $gitStatus) {
    if ([string]::IsNullOrWhiteSpace($line)) {
        continue
    }

    $pathPart = $line.Substring([Math]::Min(3, $line.Length)).Trim()
    if (-not [string]::IsNullOrWhiteSpace($pathPart)) {
        $changedFiles += $pathPart
    }
}

$recentFiles = Get-ChildItem -Path $resolvedRoot -Recurse -File -ErrorAction SilentlyContinue |
    Where-Object {
        $_.FullName -notmatch "\\.git\\" -and
        $_.FullName -notmatch "\\node_modules\\" -and
        $_.FullName -notmatch "\\.codex\\handoffs\\"
    } |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 10

$fileLines = @()
if ($changedFiles.Count -gt 0) {
    $fileLines += $changedFiles | ForEach-Object { "- $_" }
} elseif ($recentFiles.Count -gt 0) {
    $fileLines += $recentFiles | ForEach-Object {
        $relative = Resolve-Path -LiteralPath $_.FullName -Relative
        "- $relative"
    }
} else {
    $fileLines += "- No relevant files were detected."
}

$statusLines = @(
    "- Current state: $Status.",
    "- Generated automatically on $($timestamp.ToString("yyyy-MM-dd HH:mm:ss"))."
)

if ($branch -ne "unknown") {
    $statusLines += "- Git branch: $branch."
}

$changeLines = @()
if ($gitStatus.Count -gt 0) {
    $changeLines += $gitStatus | ForEach-Object { "- $_" }
} else {
    $changeLines += "- No git-tracked workspace changes were detected."
}

$resumePrompt = "Continue from `"$outputPath`". Inspect the listed files first, review git status, then complete the remaining work and verify the result."

$content = @(
    "# Current Work Context",
    "",
    "## Task",
    "- $TaskSummary",
    "",
    "## Status",
    $statusLines,
    "",
    "## Files",
    $fileLines,
    "",
    "## Changes Made",
    $changeLines,
    "",
    "## Verification",
    "- No automated verification was captured by this auto-save script.",
    "",
    "## Open Items",
    "- Review the latest Codex conversation to recover exact intent and any unresolved edge cases.",
    "- Confirm whether all changed files belong to the active task before continuing.",
    "",
    "## Next Steps",
    "1. Open this handoff file and read the listed files.",
    "2. Run `git status` in the project root to confirm the current workspace state.",
    "3. Resume implementation or cleanup, then perform task-specific verification.",
    "",
    "## Resume Prompt",
    "- $resumePrompt"
)

Set-Content -Path $outputPath -Value $content -Encoding UTF8
Write-Output $outputPath
