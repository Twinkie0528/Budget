<#
.SYNOPSIS
    Sets up Git hooks for the Budget Platform project.

.DESCRIPTION
    Installs pre-commit and pre-push hooks for code quality checks.

.EXAMPLE
    .\setup-hooks.ps1
#>

$ErrorActionPreference = "Stop"

Write-Host "Setting up Git hooks..." -ForegroundColor Cyan

$repoRoot = git rev-parse --show-toplevel
$hooksDir = Join-Path $repoRoot ".git/hooks"

# Pre-commit hook
$preCommitHook = @'
#!/bin/sh
# Pre-commit hook for Budget Platform

echo "Running pre-commit checks..."

# Check for dotnet format issues
echo "Checking .NET formatting..."
dotnet format src/BudgetPlatform.sln --verify-no-changes --verbosity minimal
if [ $? -ne 0 ]; then
    echo "ERROR: .NET formatting issues found. Run 'dotnet format src/BudgetPlatform.sln' to fix."
    exit 1
fi

# Check for frontend lint issues
if [ -d "src/web/node_modules" ]; then
    echo "Checking frontend lint..."
    cd src/web && npm run lint
    if [ $? -ne 0 ]; then
        echo "ERROR: Frontend lint issues found. Run 'npm run lint' in src/web to see details."
        exit 1
    fi
    cd ../..
fi

echo "Pre-commit checks passed!"
exit 0
'@

$preCommitPath = Join-Path $hooksDir "pre-commit"
$preCommitHook | Out-File -FilePath $preCommitPath -Encoding utf8 -NoNewline
Write-Host "Created pre-commit hook" -ForegroundColor Green

# Pre-push hook
$prePushHook = @'
#!/bin/sh
# Pre-push hook for Budget Platform

echo "Running pre-push checks..."

# Run unit tests
echo "Running unit tests..."
dotnet test tests/Budget.Tests.Unit --configuration Release --verbosity minimal --no-build 2>/dev/null || dotnet test tests/Budget.Tests.Unit --configuration Release --verbosity minimal
if [ $? -ne 0 ]; then
    echo "ERROR: Unit tests failed. Fix failing tests before pushing."
    exit 1
fi

echo "Pre-push checks passed!"
exit 0
'@

$prePushPath = Join-Path $hooksDir "pre-push"
$prePushHook | Out-File -FilePath $prePushPath -Encoding utf8 -NoNewline
Write-Host "Created pre-push hook" -ForegroundColor Green

# Make hooks executable (for Git Bash / WSL)
if (Get-Command chmod -ErrorAction SilentlyContinue) {
    chmod +x $preCommitPath
    chmod +x $prePushPath
}

Write-Host ""
Write-Host "Git hooks installed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Hooks installed:" -ForegroundColor Cyan
Write-Host "  - pre-commit: Format checking" -ForegroundColor White
Write-Host "  - pre-push: Unit tests" -ForegroundColor White
Write-Host ""
Write-Host "To skip hooks temporarily, use: git commit --no-verify" -ForegroundColor Yellow

