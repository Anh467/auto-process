@echo off
REM Setup script for Windows developers to enable auto-format on commit
REM Run this script once after cloning the repository

echo ========================================
echo Setting up Git hooks for auto-formatting
echo ========================================
echo.

REM Check if .git directory exists
if not exist ".git" (
    echo Error: .git directory not found. Make sure you're in the root of a Git repository.
    exit /b 1
)

REM Copy pre-commit hook
echo Copying pre-commit hook...
if exist ".git\hooks\pre-commit" (
    echo Pre-commit hook already exists.
) else (
    echo Creating pre-commit hook...
    copy ".git\hooks\pre-commit.sample" ".git\hooks\pre-commit"
)

echo.
echo ========================================
echo Setup complete!
echo ========================================
echo.
echo The pre-commit hook will automatically format your code when you commit.
echo.
echo Tools used:
echo - Prettier for client files (JS/TS/JSX/TSX)
echo - dotnet format for server files (.cs)
echo.
echo Requirements:
echo - Node.js and npm (for Prettier)
echo - .NET SDK 6.0+ (for dotnet format)
echo.
echo If you need to bypass the hook (not recommended), use:
echo git commit --no-verify
echo.