@echo off
setlocal enabledelayedexpansion

:: Get current git branch
for /f "tokens=*" %%b in ('git rev-parse --abbrev-ref HEAD 2^>nul') do set branch=%%b
if "!branch!"=="" set branch=detached

:: Check if there are uncommitted changes
git status --porcelain 2>nul | findstr /r . >nul
if !errorlevel! equ 0 (
    set status= *
) else (
    set status=
)

:: Get current directory name
for %%i in (.) do set path=%%~nxi

:: Output status line
echo [!branch!!status!] !path!