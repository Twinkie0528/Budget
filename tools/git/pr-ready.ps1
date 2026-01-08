<#
.SYNOPSIS
    Checks if the current branch is ready for a pull request.

.DESCRIPTION
    Runs all checks locally that would be run in CI to ensure PR readiness.

.EXAMPLE
    .\pr-ready.ps1
#>

$ErrorActionPreference = "Continue"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  PR Readiness Check" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$repoRoot = git rev-parse --show-toplevel
Set-Location $repoRoot

$allPassed = $true

# Check 1: No uncommitted changes
Write-Host "[1/6] Checking for uncommitted changes..." -ForegroundColor Yellow
$status = git status --porcelain
if ($status) {
    Write-Host "  WARN: You have uncommitted changes" -ForegroundColor Yellow
    $status | ForEach-Object { Write-Host "    $_" -ForegroundColor Gray }
} else {
    Write-Host "  OK: Working directory clean" -ForegroundColor Green
}

# Check 2: Branch is up to date
Write-Host "[2/6] Checking if branch is up to date..." -ForegroundColor Yellow
git fetch origin --quiet
$behind = git rev-list HEAD..origin/main --count 2>$null
if ($behind -gt 0) {
    Write-Host "  WARN: Branch is $behind commits behind main" -ForegroundColor Yellow
} else {
    Write-Host "  OK: Branch is up to date" -ForegroundColor Green
}

# Check 3: Backend builds
Write-Host "[3/6] Building backend..." -ForegroundColor Yellow
$buildOutput = dotnet build src/BudgetPlatform.sln --configuration Release --verbosity quiet 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "  FAIL: Backend build failed" -ForegroundColor Red
    $allPassed = $false
} else {
    Write-Host "  OK: Backend builds successfully" -ForegroundColor Green
}

# Check 4: Backend tests
Write-Host "[4/6] Running backend unit tests..." -ForegroundColor Yellow
$testOutput = dotnet test tests/Budget.Tests.Unit --configuration Release --verbosity quiet --no-build 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "  FAIL: Unit tests failed" -ForegroundColor Red
    $allPassed = $false
} else {
    Write-Host "  OK: Unit tests pass" -ForegroundColor Green
}

# Check 5: Frontend builds
Write-Host "[5/6] Building frontend..." -ForegroundColor Yellow
if (Test-Path "src/web/package.json") {
    Push-Location "src/web"
    
    if (-not (Test-Path "node_modules")) {
        Write-Host "    Installing dependencies..." -ForegroundColor Gray
        npm ci --silent 2>&1 | Out-Null
    }
    
    $buildOutput = npm run build 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  FAIL: Frontend build failed" -ForegroundColor Red
        $allPassed = $false
    } else {
        Write-Host "  OK: Frontend builds successfully" -ForegroundColor Green
    }
    
    Pop-Location
} else {
    Write-Host "  SKIP: No frontend found" -ForegroundColor Gray
}

# Check 6: Lint
Write-Host "[6/6] Running linters..." -ForegroundColor Yellow
if (Test-Path "src/web/package.json") {
    Push-Location "src/web"
    $lintOutput = npm run lint 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  FAIL: Frontend lint errors" -ForegroundColor Red
        $allPassed = $false
    } else {
        Write-Host "  OK: No lint errors" -ForegroundColor Green
    }
    Pop-Location
}

# Summary
Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
if ($allPassed) {
    Write-Host "  All checks passed! Ready for PR." -ForegroundColor Green
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Create PR with:" -ForegroundColor Yellow
    $branch = git branch --show-current
    Write-Host "  gh pr create --title 'Your PR title' --body 'Description'" -ForegroundColor White
    Write-Host ""
    exit 0
} else {
    Write-Host "  Some checks failed. Please fix before creating PR." -ForegroundColor Red
    Write-Host "============================================" -ForegroundColor Cyan
    exit 1
}

