param(
    [string]$ProjectRoot = (Get-Location).Path,
    [string]$TaskSummary = "Resume the most recent Codex task in this project.",
    [string]$Status = "in progress",
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$CodexArgs
)

$ErrorActionPreference = "Stop"

$resolvedRoot = (Resolve-Path $ProjectRoot).Path
$saveScript = Join-Path $PSScriptRoot "save-work-context.ps1"

if (-not (Test-Path $saveScript)) {
    throw "Missing save script: $saveScript"
}

Push-Location $resolvedRoot
try {
    & codex @CodexArgs
    $codexExitCode = $LASTEXITCODE
} finally {
    $handoffPath = & $saveScript -ProjectRoot $resolvedRoot -TaskSummary $TaskSummary -Status $Status
    Write-Host "Saved work context to: $handoffPath"
    Pop-Location
}

exit $codexExitCode
